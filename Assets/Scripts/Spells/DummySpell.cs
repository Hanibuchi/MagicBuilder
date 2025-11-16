using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

// SpellBaseの抽象メソッドを実装するための最小限のダミークラス
public class DummySpell : SpellBase
{
    public int[] _offsets;

    public DummySpell(string name, int[] offsets)
    {
        spellName = name;
        _offsets = offsets;
    }

    // テスト用のオフセットを返す
    public override int[] GetNextSpellOffsets(List<SpellBase> wandSpells, int currentSpellIndex)
    {
        return _offsets;
    }

    // 抽象メソッドのダミー実装
    public override void DisplayAimingLine(List<SpellBase> wandSpells, int currentSpellIndex, float rotationZ, float strength, Vector2 casterPosition, bool clearLine = false) { }
    public override void FireSpell(List<SpellBase> wandSpells, int currentSpellIndex, float rotationZ, float strength, SpellContext context) { }
}