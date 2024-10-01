# 检查操作系统类型
if ($IsWindows) {
    # Windows 平台构建命令
    dotnet publish -c Release -r win-x64 -p:PublishAot=True
} else {
    # Linux 平台构建命令
    dotnet publish -c Release -r linux-x64 -p:PublishAot=True
}
