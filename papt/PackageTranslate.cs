namespace papt
{
    using Papt;
    using System.Collections.Generic;

    /// <summary>
    /// Defines the <see cref="PackageTranslate" />
    /// </summary>
    public class PackageTranslate
    {
        /// <summary>
        /// Defines the package_translate_list
        /// </summary>
        public static Dictionary<string, string> package_translate_list = new(){
            {"build-essential","base-devel"}
            //TODO
        };

        /// <summary>
        /// The TranslatePackage
        /// </summary>
        /// <param name="packages">The packages<see cref="List{string}"/></param>
        /// <param name="pass_check">The pass_check<see cref="bool"/></param>
        /// <returns>The <see cref="List{string}"/></returns>
        public static List<string> TranslatePackage(List<string> packages, bool pass_check = false)
        {
            Logger.Debug($"{packages}");
            var neo_packages = new List<string>(packages);
            foreach (string package in packages)
            {
                foreach (string key in package_translate_list.Keys)
                {
                    if (key == package)
                    {
                        Logger.Warning($"Can you want install {package_translate_list[key]}? [Y/n]");
                        if (pass_check || Utilities.ISConsoleInputY(true))
                        {
                            neo_packages.Remove(package);
                            neo_packages.Add(package_translate_list[key]);
                        }
                    }
                }
            }
            return neo_packages;
        }
    }
}
