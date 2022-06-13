# Deer_GameFramework_HuaTuo
基于GameFramework框架衍生的一个huatuo热更框架，实现除GameFramework库底层代码以及更新流程逻辑层代码，其余流程及义务层代码全部热更。

### 热更框架流程图

![流程](D:\Working\GitHub\Deer_GameFramework_HuaTuo\DescDocu\流程.png)

### 热更程序集

目前热更程序集有四个，可在业务层扩展

- HotfixMain.dll  入口程序集

- HotfixCommon.dll 公共程序集

- HotFixFramework.Runtime.dll 框架程序集

- HotfixBusiness.dll 业务程序集

  可扩展程序集

- HotfixBusinessA.dll 业务程序集A

- HotfixBusinessB.dll 业务程序集B

程序集打AssetBundle（AB），遵循GameFramework（GF）资源管理,以及集成到GF，AB自动生成程序集，并打出ab资源。

**致谢仓库**

[EllanJiang](https://github.com/EllanJiang)/**[GameFramework](https://github.com/EllanJiang/GameFramework)**

[focus-creative-games](https://github.com/focus-creative-games)/**[huatuo](https://github.com/focus-creative-games/huatuo)**

[pirunxi](https://github.com/pirunxi)/**[il2cpp_huatuo](https://github.com/pirunxi/il2cpp_huatuo)**
