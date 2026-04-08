$files = Get-ChildItem -Path "Assets\Resources\Spells" -Filter "*.asset"
$spells = @()

foreach ($f in $files) {
    $content = Get-Content $f.FullName -Raw
    
    $nameMatch = [regex]::Match($content, 'spellName:\s*"?([^"\r\n]+)"?')
    $name = if ($nameMatch.Success) { $nameMatch.Groups[1].Value } else { "" }
    
    $descMatch = [regex]::Match($content, 'spellDescription:\s*"?([^"\r\n]+)"?')
    $desc = if ($descMatch.Success) { $descMatch.Groups[1].Value } else { "" }
    
    $typeMatch = [regex]::Match($content, 'spellType:\s*([^\r\n]+)')
    $type = if ($typeMatch.Success) { $typeMatch.Groups[1].Value.Trim() } else { "" }
    
    $catMatch = [regex]::Match($content, 'category:\s*([^\r\n]+)')
    $catVal = if ($catMatch.Success) { $catMatch.Groups[1].Value.Trim() } else { "0" }
    
    $rarityMatch = [regex]::Match($content, 'rarity:\s*([^\r\n]+)')
    $rarityVal = if ($rarityMatch.Success -and $rarityMatch.Groups[1].Value.Trim() -ne "") { $rarityMatch.Groups[1].Value.Trim() } else { "0" }
    
    try { $name = [System.Text.RegularExpressions.Regex]::Unescape($name) } catch {}
    try { $desc = [System.Text.RegularExpressions.Regex]::Unescape($desc) } catch {}
    
    # 改行を空白に置換
    $desc = $desc -replace '\r?\n|\r', ' '
    
    $catStr = switch ($catVal) {
        "0" { "Attack" }
        "1" { "Modifier" }
        "2" { "Branch" }
        "3" { "Other" }
        default { "Unknown($catVal)" }
    }
    
    $rarityStr = switch ($rarityVal) {
        "0" { "Common" }
        "1" { "Uncommon" }
        "2" { "Rare" }
        "3" { "Epic" }
        "4" { "Legendary" }
        default { "Common" }
    }
    
    $spells += [PSCustomObject]@{
        "ファイル名" = $f.Name
        "呪文名" = $name
        "SpellType" = $type
        "Category" = $catStr
        "Rarity" = $rarityStr
        "説明" = $desc
    }
}

$spells | Export-Csv -Path "spells_table.csv" -NoTypeInformation -Encoding UTF8
Write-Host "CSV Export Complete."