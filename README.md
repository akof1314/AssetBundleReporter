# Unity AssetBundle Reporter
Unity AssetBundle 冗余检测与资源分析
![](http://img.blog.csdn.net/20170930130444977)

## 原因
在使用 Unity 进行开发项目时，通常使用 AssetBundle 来进行资源打包，虽然在 Unity 5.x 版本里提供了更加智能的依赖自动管理，即如果依赖的资源没有显式设置 AssetBundle 名称，那么就会被隐式地打包到同一个 AssetBundle 包里面。而如果已经设置的话，那么就会自动生成依赖关系。

那么当被依赖的资源没有独立打包时，而此时又存在两个或以上 AssetBundle 依赖此资源的话，这个资源就会被同时打包到这些 AssetBundle 包里面，造成资源冗余，增大 AssetBundle 包的体积，增加游戏加载 AssetBundle 时所需的内存。

于是，检测 AssetBundle 资源的冗余，才好方便对其进行优化。检测冗余可以在未打包前对将要打包的资源做分析，但是这无法完全保证打包之后的 AssetBundle 完全无冗余，一是分析时无法保证正确无冗余，二是引用的内置资源无法剔除冗余，所以对打包之后的 AssetBundle 包进行检测才真正检查到所有的冗余。

## 实现过程
详见 http://blog.csdn.net/akof1314/article/details/78141789

## 使用说明
将插件包导入到工程，打包 AssetBundle 之后，调用检测的接口，如下所示：
```
/// <summary>
/// 分析打印 AssetBundle
/// </summary>
/// <param name="bundlePath">AssetBundle 文件所在文件夹路径</param>
/// <param name="outputPath">Excel 报告文件保存路径</param>
/// <param name="completed">分析打印完毕后的回调</param>
public static void AnalyzePrint(string bundlePath, string outputPath, UnityAction completed = null)
```
传入所需的参数即可，等待输出报告。另外注意一点，打包完 AssetBundle 就立即检测，这样才能在分析 AssetBundle 的时候，获取到正确的自定义脚本类信息，才能分析完全。过后再检测的话，自定义的脚本类可能被其他人所修改，那么就无法分析正确。Unity 5.4+ 支持场景资源分析，Unity 4.X ~ Unity 5.3 只支持非场景资源分析。