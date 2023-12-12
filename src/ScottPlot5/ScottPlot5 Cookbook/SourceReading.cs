﻿namespace ScottPlotCookbook;

internal static class SourceReading
{
    private static List<string> GetRecipeSourceFilePaths()
    {
        List<string> paths = new();

        if (!Directory.Exists(Cookbook.RecipeSourceFolder))
            throw new DirectoryNotFoundException(Cookbook.RecipeSourceFolder);

        paths.AddRange(Directory.GetFiles(Cookbook.RecipeSourceFolder, "*.cs"));

        foreach (string subFolder in Directory.GetDirectories(Cookbook.RecipeSourceFolder))
        {

            paths.AddRange(Directory.GetFiles(subFolder, "*.cs"));
        }

        if (!paths.Any())
            throw new InvalidOperationException("no source files found");

        return paths;
    }

    public static List<RecipeSource> GetRecipeSources()
    {
        const int indentationCharacters = 12;

        List<RecipeSource> sources = new();

        string recipeStartSignal = "        {";
        string recipeOverSignal = "        }";

        foreach (string path in GetRecipeSourceFilePaths())
        {
            string[] rawLines = File.ReadAllLines(path);
            string pageNameSafe = string.Empty;
            string recipeNameSafe = string.Empty;
            StringBuilder sourceBeingExtended = new();
            bool InRecipe = false;

            foreach (string line in rawLines)
            {
                if (line.StartsWith("        PageName = "))
                {
                    pageNameSafe = line.Split('"')[1];
                    continue;
                }

                if (line.StartsWith("        public override string Name ="))
                {
                    recipeNameSafe = line.Split('"')[1];
                    continue;
                }

                if (!InRecipe && line.StartsWith(recipeStartSignal))
                {
                    InRecipe = true;
                    continue;
                }

                if (InRecipe && line.StartsWith(recipeOverSignal))
                {
                    string prefix = "ScottPlot.Plot myPlot = new();";
                    string suffix = $"myPlot.SavePng(\"{UrlTools.UrlSafe(recipeNameSafe)}.png\");";
                    string sourceLines = string.Join(Environment.NewLine, sourceBeingExtended).Trim();
                    string source = $"{prefix}\n\n{sourceLines}\n\n{suffix}";
                    RecipeSource rs = new(pageNameSafe, recipeNameSafe, source);
                    sources.Add(rs);
                    InRecipe = false;
                    sourceBeingExtended.Clear();
                    continue;
                }

                bool lineIsWhitespace = line.Trim().Length == 0;
                if (InRecipe)
                    sourceBeingExtended.AppendLine(lineIsWhitespace ? "" : line.Substring(indentationCharacters));
            }
        }

        return sources;
    }
}
