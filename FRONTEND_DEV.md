# 前端开发人员手册
## <span id="t1">基础架构和约定</span>

- 前端文件根目录为`./visual_assets/kboard`。  
- `kboard`内的目录结构可以自行定义，但必须提供`kboard.html`作为弹幕展示板。  
- 前端使用使用Websocket接收来自后端的Json格式信息推送。 
  
基本执行流程如下：  
- 连接到websocket服务器[<<PLACEHOLDER_WEBSOCKET_URL>>](#t2)；
- 从服务器收到文本时，解析为json，依照[Json消息推送格式约定](#t3)解析消息；
- 渲染解析出的消息。  

## <span id="t2">页面内占位替换符</span>

后端提供一系列占位替换符，占位符将被替换为实际信息和值。  
**仅在`text/*`和`application/javascript`类型的文件中提供占位替换功能。**  
| 占位替换符 | 描述 |
|:--------| :---------:|
| <<PLACEHOLDER_WEBSOCKET_URL>> | Websocket服务器URL |
| <<PLACEHOLDER_HOST>> | 后端服务器主机名 |
| <<PLACEHOLDER_PORT>> | 后端服务器端口号 |  

后端插件有能力定义更多占位替换符，但在成为事实标准前不建议引用。

## <span id="t3">Json消息推送格式约定</span>
所有发送到后端的图片资源URL已经本地化，不会跨域。  
  
Json消息均应当包括以下节点：  
| 节点 | 描述 |
|:--------| :---------:|
| type | 消息类型，为[`normal`,`pinned`](#t3_normal),[`gift`](#t3_gift),[`crew`](#t3_crew) |
| name | 触发事件者的用户名 |
| uid | 触发事件者的UID |
| avatar | 触发事件者的头像URL |
| tags | 勋章URL列表，标识特殊身份的小图片标记。 |
| medal | 触发者佩戴的粉丝勋章(不可用时会传空节点)，具体如下： |
| medal.title | 粉丝勋章名 |  
| medal.level | 粉丝勋章等级 |  
| medal.tuid | 粉丝勋章主播UID |  
| medal.tname | 粉丝勋章主播用户名 |  
  
### <span id="t3_normal">normal/pinned类型节点</span>
`normal`(普通弹幕)和`pinned`(醒目留言，SC)类型还将提供以下节点：
| 节点 | 描述 |
|:--------| :---------:|
| content | 消息内容，需按照[富文本消息解析约定](#t4)解析表情、图片等元素 |
| name | 触发事件者的用户名 |
| uid | 触发事件者的UID |
| priority | SC保持时间。对于普通弹幕为0。 |

### <span id="t3_gift">gift类型节点</span>
`gift`(礼物赠送)类型还将提供以下节点：
| 节点 | 描述 |
|:--------| :---------:|
| gift_name | 礼物名称 |
| gift_count | 礼物数量 |
| gift_cost | 礼物消耗的虚拟货币数量 |
| gift_is_gold | 礼物消耗的是否是金瓜子（电池） |
| img_url | 礼物展示图片URL。不可用时此节点不存在。 |  

### <span id="t3_crew">crew类型节点</span>
`crew`(舰长购买)类型还将提供以下节点：
| 节点 | 描述 |
|:--------| :---------:|
| ctype | 类型，`1`总督，`2`提督，`3`舰长 |
| len | 购买时长，单位为月 |

## <span id="t4">富文本消息解析约定</span>
文本中出现符合以下格式的内容时，应当解析为对应的多媒体内容：
| 占位符 | 描述 |
|:--------| :---------:|
| [img:http://xxx/xxx/xxx] | 显示给定url的图片。在后端实现中，B站个性表情也属于图片。 |
| [emoj:http://xxx/xxx/xxx] | 显示给定url的表情。在后端实现中，B站基础表情属于此表情。 |
  
URL均经过后端处理，不会跨域。

## 参考实现
您可以查看前端的一个参考实现：
[OpenDanmaki/visual_assets/kboard/kboard.html](OpenDanmaki/visual_assets/kboard/kboard.html)  
*作者不太擅长前端，此参考实现仅展示上述文档所述部分逻辑的实现，可能不是最优方案。*