$markdownFile = "../README.md"
$outputFile = "./Readme.cs"

$content = Get-Content $markdownFile -Raw
$escapedContent = $content -replace '"', '\"'  # 转义引号

$csContent = @"
//! 这个文件只是占位符 防止构建失败 会在构建时生成 不要修改它
namespace papt
{
    public static class ReadmeConstants
    {
        public const string Content = @"$escapedContent";
    }
}
"@

Set-Content $outputFile -Value $csContent