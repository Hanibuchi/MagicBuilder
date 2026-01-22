using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

[CustomPropertyDrawer(typeof(EnemySpawnerConfig), true)]
public class EnemySpawnerPropertyDrawer : PropertyDrawer
{
    private static Type[] _derivedTypes;
    private static string[] _typeNames;

    private void InitializeTypes()
    {
        if (_derivedTypes == null)
        {
            _derivedTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(EnemySpawnerConfig).IsAssignableFrom(p) && !p.IsAbstract)
                .ToArray();

            _typeNames = _derivedTypes.Select(t => t.Name).ToArray();
        }
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        InitializeTypes();

        EditorGUI.BeginProperty(position, label, property);

        // 現在の型のインデックスを特定
        int currentIndex = 0;
        string fullTypeName = property.managedReferenceFullTypename;
        if (!string.IsNullOrEmpty(fullTypeName))
        {
            string typeName = fullTypeName.Split(' ').Last().Split('.').Last();
            currentIndex = Array.IndexOf(_typeNames, typeName);
            if (currentIndex < 0) currentIndex = 0;
        }

        // ラベルの描画
        Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

            // 型選択用のポップアップ(Enumのように動作)
            Rect typeRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, EditorGUIUtility.singleLineHeight);
            int nextIndex = EditorGUI.Popup(typeRect, "Spawner Type", currentIndex, _typeNames);

            if (nextIndex != currentIndex)
            {
                // 型が変更されたらインスタンスを作成
                Type selectedType = _derivedTypes[nextIndex];
                property.managedReferenceValue = Activator.CreateInstance(selectedType);
            }

            // インスタンスの各フィールドを自動描画
            SerializedProperty child = property.Copy();
            SerializedProperty end = property.GetEndProperty();
            
            float currentY = typeRect.y + EditorGUIUtility.singleLineHeight + 2;
            
            if (child.NextVisible(true)) // 子要素へ移動
            {
                do
                {
                    if (SerializedProperty.EqualContents(child, end)) break;

                    float height = EditorGUI.GetPropertyHeight(child, true);
                    Rect childRect = new Rect(position.x, currentY, position.width, height);
                    EditorGUI.PropertyField(childRect, child, true);
                    currentY += height + 2;
                }
                while (child.NextVisible(false));
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded)
            return EditorGUIUtility.singleLineHeight;

        float height = EditorGUIUtility.singleLineHeight * 2 + 10; // Foldout + Popup + Spacing

        SerializedProperty child = property.Copy();
        SerializedProperty end = property.GetEndProperty();

        if (child.NextVisible(true))
        {
            do
            {
                if (SerializedProperty.EqualContents(child, end)) break;
                height += EditorGUI.GetPropertyHeight(child, true) + 2;
            }
            while (child.NextVisible(false));
        }

        return height;
    }
}
