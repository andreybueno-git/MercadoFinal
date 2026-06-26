#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.Collections.Generic;
using UnityEngine;

// BuildTool.cs — gera o executável do jogo. Menu "Tudo Tem Preço → Gerar .exe (Windows)" ou
// via linha de comando: -executeMethod BuildTool.BuildWindows. Sai na pasta build/ na raiz.
public static class BuildTool {
    [MenuItem("Tudo Tem Preço/Gerar .exe (Windows)")]
    public static void BuildWindows() {
        var scenes = new List<string>();
        foreach (var s in EditorBuildSettings.scenes) if (s.enabled) scenes.Add(s.path);
        if (scenes.Count == 0) { Debug.LogError("Sem cenas no Build Settings. Rode 'Criar Cenas do Jogo' antes."); return; }

        string outPath = "build/MercadoFinal.exe";
        var opt = new BuildPlayerOptions {
            scenes = scenes.ToArray(),
            locationPathName = outPath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };
        var report = BuildPipeline.BuildPlayer(opt);
        var s2 = report.summary;
        if (s2.result == BuildResult.Succeeded)
            Debug.Log("BUILD_OK tamanho=" + s2.totalSize + " saida=" + outPath);
        else
            Debug.LogError("BUILD_FALHOU resultado=" + s2.result + " erros=" + s2.totalErrors);
    }
}
#endif
