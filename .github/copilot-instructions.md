# GitHub Copilot Instructions for Make War Not Love (Continued)

## Mod Overview and Purpose

Make War Not Love (Continued) is an updated version of a RimWorld mod that disables social relationships for colonists. The goal of this mod is to optimize performance at mid-late game by reducing the load caused by social interactions and relationships.

## Key Features and Systems

- Disables social relationships for colonists to improve performance in late game.
- Patches existing XML definitions to modify game behavior.
- Uses Harmony patching to override existing code.

## Coding Patterns and Conventions

- The mod follows standard C# coding conventions, with consistent naming and formatting.
- XML files are used to define patches for existing game definitions.
- Harmony patching is used to override existing code and add new functionality.
- The mod is organized into several C# files, each representing a different aspect of the mod's functionality.

## XML Integration

The mod uses XML patches to modify existing game definitions. The XML files are located in the `Patches` folder and are structured as follows:

xml
<?xml version="1.0" encoding="utf-8" ?>
<Defs>
  <PawnRelationDef>
    <label>Spouse</label>
    <defName>Spouse</defName>
    <inverse>Spouse</inverse>
    <visibility>Hidden</visibility>
    <exclusive>True</exclusive>
  </PawnRelationDef>
</Defs>


The XML files use the same schema as the original game files, allowing them to be easily integrated with the game.

## Harmony Patching

The mod uses Harmony patching to override existing code and add new functionality. The Harmony patches are located in the C# files and are structured as follows:

csharp
[HarmonyPatch(typeof(Pawn_RelationsTracker), "AddRelation", typeof(RelationDef), typeof(Pawn))]
public static class Patch_Pawn_RelationsTracker_AddRelation
{
    public static bool Prefix(Pawn_RelationsTracker __instance, RelationDef relation, Pawn otherPawn)
    {
        if (relation.defName == "Spouse")
        {
            return false;
        }

        return true;
    }
}


The Harmony patches use the `HarmonyPatch` attribute to specify the method to be patched and the `Prefix` method to override the existing code. The `Prefix` method returns `false` to prevent the existing code from executing, effectively disabling the specified functionality.

## Suggestions for Copilot

GitHub Copilot can be a useful tool for modding in RimWorld. Here are some suggestions for how Copilot can be used:

- Use Copilot to generate XML patches for existing game definitions.
- Use Copilot to generate Harmony patches to override existing code.
- Use Copilot to generate new C# classes and methods for mod functionality.
- Use Copilot to generate code snippets for common mod tasks, such as creating new thoughts or interaction workers.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).
- When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.
- When mentioning the rimworld log, automatically use the Rimworld MCP service to fetch the log.

