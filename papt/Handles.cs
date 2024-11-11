using Papt;

internal static class Handles
{

    public static void HandleClean(string packageManager, string confirmFlag)
    {
        PackageManagerScript.RunCommand($"{packageManager} -Sc {confirmFlag}");
    }

    public static void HandleError(string command)
    {
        Logger.Error($"Unknown command '{command}'");
        HandleShowUsage();
    }

    public static void HandleInstall(string packageManager, bool havePackage, string packageString, string confirmFlag)
    {
        if (!havePackage)
        {
            HandleShowUsage();
            return;
        }
        PackageManagerScript.RunCommand($"{packageManager} -S {packageString} {confirmFlag}");
    }

    public static void HandleList(string packageManager)
    {
        PackageManagerScript.RunCommand($"{packageManager} -Q");
    }

    public static void HandleRemove(string packageManager, bool havePackage, string packageString, string confirmFlag)
    {
        if (!havePackage)
        {
            HandleShowUsage();
            return;
        }
        PackageManagerScript.RunCommand($"{packageManager} -R {packageString} {confirmFlag}");
    }

    public static void HandleSearch(string packageManager, bool havePackage, string packageString)
    {
        if (!havePackage)
        {
            HandleShowUsage();
            return;
        }
        PackageManagerScript.RunCommand($"{packageManager} -Ss {packageString}");
    }

    public static void HandleShow(string packageManager, bool havePackage, string packageString)
    {
        if (!havePackage)
        {
            HandleShowUsage();
            return;
        }
        PackageManagerScript.RunCommand($"{packageManager} -Qi {packageString}");
    }

    public static void HandleShowUsage()
    {
        //Console.WriteLine("Usage: <command> [options] [packages]");
        // 显示更多帮助信息
        HelpUtility.ShowUsage();
    }

    public static void HandleUpdate(string packageManager)
    {
        PackageManagerScript.RunCommand($"{packageManager} -Sy");
    }

    public static void HandleUpgrade(string packageManager, bool havePackage, string packageString, string confirmFlag)
    {
        if (havePackage)
        {
            PackageManagerScript.RunCommand($"{packageManager} -Syu {packageString} {confirmFlag}");
        }
        else
        {
            PackageManagerScript.RunCommand($"{packageManager} -Su {confirmFlag}");
        }
    }

    public static void HandleRAWcommand(string packageManager, string raw_command){
        PackageManagerScript.RunCommand($"{packageManager} {raw_command}");
    }
    public static void HandleAutoRemove(string packageManager, bool havePackage, string packageString, string confirmFlag)
    {
        if (!havePackage)
        {
            HandleShowUsage();
            return;
        }
        PackageManagerScript.RunCommand($"{packageManager} -R {packageString} {confirmFlag}");
        PackageManagerScript.RunCommand($"{packageManager} -Rns $({packageManager} -Qdtq) {confirmFlag}");
    }
}