# 定义公共参数
$configuration = "Release"
$selfContained = $true
$publishAot = $true

# 设置运行时标识符
if ($IsWindows) {
    $runtime = "win-x64"
    $executableName = "papt.exe"
} else {
    $runtime = "linux-x64"
    $executableName = "papt"
}

# 计算输出路径（统一使用 POSIX 路径格式）
$outputDir = "./bin/$configuration/$runtime/publish"

# 执行发布命令
dotnet publish -c $configuration `
    -r $runtime `
    -o $outputDir `
    --self-contained $selfContained `
    -p:PublishAot=$publishAot `
    -p:DebugType=None `
    -p:DebugSymbols=false

# 验证输出文件
if (Test-Path "$outputDir/$executableName") {
    Write-Host "✅ Build success! Output: $outputDir/$executableName"
    exit 0
} else {
    Write-Host "❌ Build failed! File not found: $executableName"
    exit 1
}