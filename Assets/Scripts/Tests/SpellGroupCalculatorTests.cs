using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

[TestFixture]
public class SpellGroupCalculatorTests
{
    // テスト対象の静的メソッドをラップ（または直接ここに定義されたと仮定）
    // 実際の実装場所に応じて、この行は調整してください。
    private int[] CalculateIndices(List<SpellBase> wandSpells, int currentSpellIndex, int[] relativeGroupOffsets)
    {
        return SpellBase.GetAbsoluteIndicesFromSpellGroupArray(wandSpells, currentSpellIndex, relativeGroupOffsets);
    }

    // WandSpells の作成ヘルパー関数
    private List<SpellBase> CreateWand(params (string name, int[] offsets)[] spellDefinitions)
    {
        // return spellDefinitions.Select(def => (SpellBase)ScriptableObject.CreateInstance<DummySpell>()).ToList();
        // // 実際には、以下のようにインスタンスを生成する
        // // return spellDefinitions.Select(def => new DummySpell(def.name, def.offsets)).ToList();

        // Unity Editor外でのテストを簡単にするため、ここではダミーオブジェクトを直接生成する
        List<SpellBase> wand = new List<SpellBase>();
        foreach (var def in spellDefinitions)
        {
            var dummy = ScriptableObject.CreateInstance<DummySpell>();
            dummy.name = def.name;
            dummy._offsets = def.offsets;
            wand.Add(dummy);
        }
        return wand;
    }

    // --- テストケース ---

    [Test]
    public void TestCase_SimpleNextGroup()
    {
        // 杖: [0: MultCast({1}), 1: SpellA({0}), 2: SpellB({0}), 3: SpellC({0})]
        var wand = CreateWand(
            ("MultCast", new int[] { 1 }),  // 0
            ("SpellA", new int[] { 0 }),    // 1
            ("SpellB", new int[] { 0 }),    // 2
            ("SpellC", new int[] { 0 })     // 3
        );
        int current = 0;
        int[] offsets = { 1 }; // 次のまとまり(1)

        // 期待値: 呪文0までfired=true。次にfired=falseなのはインデックス1。これが1つ目のグループの先頭。
        int[] expected = { 1 };
        int[] actual = CalculateIndices(wand, current, offsets);
        Assert.That(actual, Is.EqualTo(expected), "Case 1: 直後の単発呪文");
    }

    [Test]
    public void TestCase_SkipGroup()
    {
        // 杖: [0: MultCast({2}), 1: SpellA({+1}), 2: SpellB({0}), 3: SpellC({0}), 4: SpellD({0})]
        // グループ1: 1 -> 2 (つまり{1, 2})
        // グループ2: 3
        var wand = CreateWand(
            ("MultCast", new int[] { 1 }),  // 0
            ("SpellA", new int[] { 1 }),    // 1. これが2を呼び出す
            ("SpellB", new int[] { 0 }),    // 2
            ("SpellC", new int[] { 0 }),    // 3
            ("SpellD", new int[] { 0 })     // 4
        );
        int current = 0;
        int[] offsets = { 2 }; // 2つ目のまとまりを呼び出す

        // 期待値:
        // i=1: start=1。DFS: 1 -> 2。fired={T, T, T, F, F}。
        // i=2: start=3。これが2つ目のグループの先頭。
        int[] expected = { 3 };
        int[] actual = CalculateIndices(wand, current, offsets);
        Assert.That(actual, Is.EqualTo(expected), "Case 2: 最初の連鎖グループをスキップ");
    }

    [Test]
    public void TestCase_MultipleTargets()
    {
        // 杖: [0: MultCast({1, 3}), 1: SpellA({0}), 2: SpellB({+1}), 3: SpellC({0}), 4: SpellD({0}), 5: SpellE({0})]
        // グループ1: 1
        // グループ2: 2 -> 3
        // グループ3: 4
        var wand = CreateWand(
            ("MultCast", new int[] { 1 }),  // 0
            ("SpellA", new int[] { 0 }),    // 1
            ("SpellB", new int[] { 1 }),    // 2. これが3を呼び出す
            ("SpellC", new int[] { 0 }),    // 3
            ("SpellD", new int[] { 0 }),    // 4
            ("SpellE", new int[] { 0 })     // 5
        );
        int current = 0;
        int[] offsets = { 1, 3 }; // 1つ目と3つ目のまとまりを呼び出す

        // 期待値:
        // i=1: start=1。ターゲットに1を追加。DFS: 1。fired={T, T, F, F, F, F}。
        // i=2: start=2。DFS: 2 -> 3。fired={T, T, T, T, F, F}。
        // i=3: start=4。ターゲットに4を追加。DFS: 4。fired={T, T, T, T, T, F}。
        int[] expected = { 1, 4 };
        int[] actual = CalculateIndices(wand, current, offsets);
        Assert.That(actual, Is.EqualTo(expected), "Case 3: 複数のグループを呼び出す");
    }

    [Test]
    public void TestCase_DeeplyNestedGroup()
    {
        // 杖: [0: MultCast({2}), 1: A(+1), 2: B(+1), 3: C(+1), 4: D(0), 5: E(0)]
        // グループ1: 1 -> 2 -> 3 -> 4 (つまり{1, 2, 3, 4})
        // グループ2: 5
        var wand = CreateWand(
            ("MultCast", new int[] { 2 }),  // 0
            ("A", new int[] { 1 }),         // 1
            ("B", new int[] { 1 }),         // 2
            ("C", new int[] { 1 }),         // 3
            ("D", new int[] { 0 }),         // 4
            ("E", new int[] { 0 })          // 5
        );
        int current = 0;
        int[] offsets = { 2 }; // 2つ目のまとまり

        // 期待値:
        // i=1: start=1。DFS: 1 -> 2 -> 3 -> 4。fired={T, T, T, T, T, F}。
        // i=2: start=5。これが2つ目のグループの先頭。
        int[] expected = { 5 };
        int[] actual = CalculateIndices(wand, current, offsets);
        Assert.That(actual, Is.EqualTo(expected), "Case 4: 深くネストされたグループをスキップ");
    }

    [Test]
    public void TestCase_OutOfRangeOffset()
    {
        // 杖: [0: MultCast({1, 5}), 1: A(0), 2: B(0)]
        var wand = CreateWand(
            ("MultCast", new int[] { 1, 5 }),  // 0
            ("A", new int[] { 0 }),         // 1
            ("B", new int[] { 0 })          // 2
        );
        int current = 0;
        int[] offsets = { 1, 5 }; // 5は存在しない

        // 期待値:
        // i=1: start=1。ターゲットに1を追加。DFS: 1。fired={T, T, F}。
        // i=2: start=2。DFS: 2。fired={T, T, T}。
        // i=3: start= -1 (範囲外)。 break。
        int[] expected = { 1 };
        int[] actual = CalculateIndices(wand, current, offsets);
        Assert.That(actual, Is.EqualTo(expected), "Case 5: 範囲外のオフセットを無視");
    }

    [Test]
    public void TestCase_ComplexOffsetsAndOrdering()
    {
        // 杖: [0: MultCast({2, 1}), 1: A(+1, +2), 2: B(0), 3: C(0), 4: D(0)]
        // グループ1: 1 -> 2, 3 (つまり{1, 2, 3})
        // グループ2: 4
        var wand = CreateWand(
            ("MultCast", new int[] { 2, 1 }),  // 0
            ("A", new int[] { 1, 2 }),      // 1. これが2と3を呼び出す
            ("B", new int[] { 0 }),         // 2
            ("C", new int[] { 0 }),         // 3
            ("D", new int[] { 0 })          // 4
        );
        int current = 0;
        int[] offsets = { 2, 1 }; // オフセットの順序はソートされるため {1, 2} と同じ

        // 期待値:
        // i=1: start=1。ターゲットに1を追加。DFS: 1 -> 2, 3。fired={T, T, T, T, F}。
        // i=2: start=4。ターゲットに4を追加。DFS: 4。fired={T, T, T, T, T}。
        int[] expected = { 1, 4 };
        int[] actual = CalculateIndices(wand, current, offsets);
        Assert.That(actual, Is.EqualTo(expected), "Case 6: 複雑なグループ構造とオフセット順序");
    }


    [Test]
    public void TestCase_StartIndexChange()
    {
        // 杖: [0: MultCast({1}), 1: SpellA({1}), 2: SpellB({0}), 3: SpellC({0})]
        var wand = CreateWand(
            ("SpellA", new int[] { 1 }),    // 1
            ("MultCast", new int[] { 1 }),  // 0
            ("SpellB", new int[] { 1 }),    // 2
            ("SpellC", new int[] { 0 }),     // 3
            ("SpellD", new int[] { 0 })    // 4
        );
        int current = 1;
        int[] offsets = { 1 }; // 次のまとまり(1)

        // 期待値: 呪文0までfired=true。次にfired=falseなのはインデックス1。これが1つ目のグループの先頭。
        int[] expected = { 2 };
        int[] actual = CalculateIndices(wand, current, offsets);
        Assert.That(actual, Is.EqualTo(expected), "Case 1: 直後の単発呪文");
    }

}