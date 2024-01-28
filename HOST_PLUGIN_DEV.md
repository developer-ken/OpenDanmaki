# 后端插件开发人员手册
## <span id="t1">基础架构和约定</span>
- 后端插件为使用dotnet 6.0开发的类库项目。其编译结果应当为`.odp.dll`。
- 后端插件应当实现`OpenDanmaki.IPlugin`接口。
  
插件加载流程：
- 内核使用`Assembly.LoadFrom(path)`加载`plugins`目录下的.odp.dll文件
- 插件的`OnPluginLoad`被调用
  
插件应当在`OnPluginLoad`中拉起自己，包括[注册相关事件](#t2)、创建必要线程等操作。

## <span id="t2">事件列表</span>
以下是可用事件列表(不一定完全)：
| 路径 | 描述 |
|:--------| :---------:|
| OpenDanmaki.DanmakuPreprocess | 在弹幕事件(包含sc、礼物等)接收后、被处理前触发。 |
| OpenDanmaki.GiftPreprocess | 弹幕礼物接收后、被处理前触发。 |
| OpenDanmaki.SuperchatPreprocess | 醒目留言接收后、被处理前触发。 |  
| OpenDanmaki.GuardEventPreprocess | 上舰事件接收后、被处理前触发。 |  
| OpenDanmaki.CommentPreprocess | 弹幕消息接收后、被处理前触发。 |  
| OpenDanmaki.MessagePrepush | 在一段Json发送到前端前触发。 |  

## <span id="t3">可访问和使用的API</span>
`OnPluginLoad`的第一个参数提供了`OpenDanmaki`的一个实例。
其中包含以下可用API(不一定完全)：
| 路径 | 描述 |
|:--------| :---------:|
| OpenDanmaki.Server | 控制内置的http/ws服务 |
| OpenDanmaki.WSMPush | 向前端推送各类消息 |
| OpenDanmaki.BiliDanmaku | 底层弹幕API，[BiliveDanmakuAgent](https://github.com/developer-ken/BiliveDanmakuAgent/blob/v2/DanmakuApi.cs) |  
| OpenDanmaki.Pluginloader | 插件加载器，可用于拉起其它插件 |  
| OpenDanmaki.TmpResourceProvider | 跨域资源加载器，用于加载并内存缓存指定资源，并提供本地可访问的URL |  
| OpenDanmaki.GiftResourcesProvider | 礼物资源加载器，可凭礼物Id获取展示图片 |  

## <span id="t4">日志管理</span>
我们使用log4net进行日志管理。`OnPluginLoad`的第二个参数为你将使用的logger。  
总是使用log4net，避免直接输出信息到Console，因为它不一定显示！

## <span id="t5">参考实现</span>
您可以通过一个最简单的插件实现更好的理解插件机制：  
[PluginDemo.odp/Plugin1.cs](PluginDemo.odp/Plugin1.cs)  
此插件监听普通弹幕事件，若发送弹幕的是舰长，就为此消息添加一个舰长图标。  
获取舰长图标的过程使用了`TmpResourceProvider`。  
添加的图标在tag列表中，显示方式由前端决定。在前端参考实现中，图标显示在用户名前方。