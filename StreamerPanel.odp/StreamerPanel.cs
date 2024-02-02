using BiliApi;
using log4net;
using OpenDanmaki;
using OpenDanmaki.Model;
using OpenDanmaki.Server;
using System.Text;

namespace StreamerPanel.odp
{
    public class StreamerPanel : IPlugin
    {
        public string PluginName => "Streamer Panel";

        public string Author => "Developer_ken";

        public Version Version => new Version(1, 0, 0, 0);

        OpenDanmaki.OpenDanmaki od_base;
        List<BiliBannedUser> bannedlist;
        DateTime lastupdate;
        ILog logger;
        bool loaded = false;
        bool isupdating = false;

        public void OnPluginLoad(OpenDanmaki.OpenDanmaki od_base, ILog logger)
        {
            HttpHandler.HandleRequest += HttpHandler_HandleRequest;
            od_base.WSMPush.MessagePrepush += WSMPush_MessagePrepush;
            this.od_base = od_base;
            bannedlist = od_base.Liveroom.manage.getBanlist();
            lastupdate = DateTime.Now;
            logger.Info("主播控制面板已初始化");
            this.logger = logger;
            loaded = true;
        }

        private void WSMPush_MessagePrepush(RawJsonEventArgs args)
        {
            if (!loaded) return;
            if (!isupdating && DateTime.Now - lastupdate > TimeSpan.FromMinutes(15))
            {
                isupdating = true;
                bannedlist = od_base.Liveroom.manage.getBanlist();
                lastupdate = DateTime.Now;
                isupdating = false;
            }

            if (args.RawJson?["type"]?.ToString() == "pinned" ||
                args.RawJson?["type"]?.ToString() == "normal")
            {
                var uid = args.RawJson.Value<long>("uid");
                if (uid == 0) return;
                bool hit = false;
                foreach (var item in bannedlist)
                {
                    if (item.uid == uid)
                    {
                        hit = true;
                        break;
                    }
                }
                if (hit)
                {
                    args.RawJson["spclass"] = "dragged-right-perm";
                }
                else
                {
                    args.RawJson["spclass"] = "";
                }
            }
        }

        private void HttpHandler_HandleRequest(OpenDanmaki.Model.HttpRequestEventArgs obj)
        {
            var e = obj.Request;
            if (obj.IsHandled) return;
            if (!e.Context.Request.RelativeURL.StartsWith("/livemgn")) return;
            var args = obj.Request.Context.Request.RelativeURL.Split('/');
            if (args.Length != 4)
            {
                e.Context.Response.ContentType = "text/html";
                e.Context.Response.SetContent(Encoding.UTF8.GetBytes(HttpHandler.FillPlaceholders(PANNEL_CONTENT,
                    ResourcesServer.Placeholders)));
                e.Context.Response.AnswerAsync().Wait();
                return;
            }
            int uid = 0;
            if (!int.TryParse(args[3], out uid)) return;
            e.Context.Response.ContentType = "text/plain";
            switch (args[2])
            {
                case "dragright":
                    {
                        if (uid <= 0) e.Context.Response.SetContent(Encoding.UTF8.GetBytes("invalid_uid"));
                        else
                        {
                            var result = od_base.Liveroom.manage.banUID(uid);
                            if (result)
                            {
                                e.Context.Response.SetContent(Encoding.UTF8.GetBytes("ok"));
                                logger.Info("禁言#" + uid);
                            }
                            else
                            {
                                e.Context.Response.SetContent(Encoding.UTF8.GetBytes("failed"));
                                logger.Info("无法禁言#" + uid);
                            }
                            lastupdate = DateTime.MinValue;
                        }
                    }
                    break;
                case "dragleft":
                    {
                        logger.Info("左滑行为未定义");
                        e.Context.Response.SetContent(Encoding.UTF8.GetBytes("nodef"));
                    }
                    break;
                case "clear":
                    {
                        var result = od_base.Liveroom.manage.debanBID(uid);
                        if (result)
                        {
                            logger.Info("解禁：#" + uid);
                            e.Context.Response.SetContent(Encoding.UTF8.GetBytes("ok"));
                        }
                        else
                        {
                            logger.Info("无法解禁" + uid);
                            e.Context.Response.SetContent(Encoding.UTF8.GetBytes("failed"));
                        }
                        lastupdate = DateTime.MinValue;
                    }
                    break;
                default:
                    return;
            }
            e.Context.Response.AnswerAsync().Wait();
            obj.IsHandled = true;
        }

        const string PANNEL_CONTENT = "<!DOCTYPE html>\r\n<html>\r\n\r\n<head>\r\n    <meta http-equiv=\"content-type\" content=\"text/html; charset=utf-8\">\r\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no\">\r\n    <style>\r\n        html {\r\n            touch-action: none;\r\n        }\r\n\r\n        .outer_container {\r\n            width: 100vw;\r\n            height: 100vh;\r\n            display: flex;\r\n            flex-direction: column;\r\n        }\r\n\r\n        .draggable {\r\n            width: 80vw;\r\n            height: 10vh;\r\n            background: lightgray;\r\n            border-radius: 10px;\r\n            position: absolute;\r\n            cursor: move;\r\n            left: 10vw;\r\n            display: flex;\r\n        }\r\n\r\n        .avatar {\r\n            margin-left: 15px;\r\n            height: 80%;\r\n            border-radius: 50%;\r\n            align-self: center;\r\n        }\r\n\r\n        .nickname {\r\n            font-weight: bold;\r\n        }\r\n\r\n        .info {\r\n            margin: 15px;\r\n        }\r\n\r\n        .hidden_field {\r\n            display: none;\r\n            height: 0px;\r\n            width: 0px;\r\n        }\r\n\r\n        .dragged-left-perm {\r\n            background: green;\r\n            transition: background 0.5s;\r\n        }\r\n\r\n        .dragged-right-perm {\r\n            background: red;\r\n            transition: background 0.5s;\r\n        }\r\n\r\n        .dragged-left {\r\n            background: green;\r\n            transition: background 0.5s;\r\n        }\r\n\r\n        .dragged-right {\r\n            background: red;\r\n            transition: background 0.5s;\r\n        }\r\n    </style>\r\n</head>\r\n\r\n<body class=\"outer_container\">\r\n    <script>\r\n        var items = new Array();\r\n        var itempointer = 0;\r\n        function PushMessage(name, message, avatar, uid, stclass) {\r\n            itempointer = itempointer % items.length;\r\n            var item = items[itempointer];\r\n            i = 0;\r\n            for (; i < 50; i++) {\r\n                if (!item.classList.contains('dragging')) break;\r\n                itempointer++;\r\n                itempointer = itempointer % items.length;\r\n                var item = items[itempointer];\r\n            }\r\n            if(i == 50) return; //没有可用的格子了\r\n            itempointer++;\r\n            item.querySelector('.avatar').src = avatar;\r\n            item.querySelector('.nickname').innerHTML = name;\r\n            item.querySelector('.message').innerHTML = message;\r\n            item.querySelector('.uid').textContent = uid;\r\n            item.querySelector('.rawinfo').textContent = message;\r\n            item.classList.remove(\"dragged-left-perm\");\r\n            item.classList.remove(\"dragged-right-perm\");\r\n            if (stclass != \"\")\r\n                item.classList.add(stclass);\r\n        }\r\n    </script>\r\n\r\n    <script>\r\n        var dragStartX;\r\n        var dragging = false;\r\n        function startDrag(e) {\r\n            var div = event.target || window.event.srcElement;\r\n            while (!div.classList.contains('draggable')) {\r\n                div = div.parentNode;\r\n            }\r\n            dragging = true;\r\n            dragStartX = e.clientX || e.touches[0].clientX;\r\n            div.classList.add('dragging');\r\n        }\r\n\r\n        function drag(e) {\r\n            var div = event.target || window.event.srcElement;\r\n            while (!div.classList.contains('draggable')) {\r\n                div = div.parentNode;\r\n            }\r\n            if (dragging) {\r\n                var clientX = e.clientX || e.touches[0].clientX;\r\n                var newLeft = div.offsetLeft - dragStartX + clientX;\r\n                if (newLeft >= 0 && newLeft <= 200) {\r\n                    div.style.left = newLeft + 'px';\r\n                    dragStartX = clientX;\r\n                }\r\n\r\n                var dragEndX = e.clientX || e.changedTouches[0].clientX;\r\n                if (dragEndX < dragStartX) {\r\n                    div.classList.remove('dragged-right');\r\n                    div.classList.add('dragged-left');\r\n                } else if (dragEndX > dragStartX) {\r\n                    div.classList.remove('dragged-left');\r\n                    div.classList.add('dragged-right');\r\n                } else {\r\n                    div.classList.remove('dragged-left');\r\n                    div.classList.remove('dragged-right');\r\n                }\r\n            }\r\n        }\r\n\r\n        function endDrag(e) {\r\n            var div = event.target || window.event.srcElement;\r\n            while (!div.classList.contains('draggable')) {\r\n                div = div.parentNode;\r\n            }\r\n            uid = div.querySelector('.uid').textContent;\r\n            var screenWidth = window.innerWidth;\r\n            div.style.left = (screenWidth / 2 - div.offsetWidth / 2) + 'px';\r\n            if (dragging) {\r\n                dragging = false;\r\n                var dragEndX = e.clientX || e.changedTouches[0].clientX;\r\n\r\n                if (dragEndX < dragStartX) {\r\n                    result = _get_api(\"/livemgn/dragleft/\" + uid);\r\n                    div.classList.remove('dragged-right')\r\n                    div.classList.remove('dragged-left')\r\n                    if (result == \"ok\") {\r\n                        addClassname(uid, 'dragged-left-perm')\r\n                        removeClassname(uid, 'dragged-right-perm')\r\n                    }\r\n                } else if (dragEndX > dragStartX) {\r\n                    result = _get_api(\"/livemgn/dragright/\" + uid);\r\n                    div.classList.remove('dragged-right')\r\n                    div.classList.remove('dragged-left')\r\n                    if (result == \"ok\") {\r\n                        addClassname(uid, 'dragged-right-perm')\r\n                        removeClassname(uid, 'dragged-left-perm')\r\n                    }\r\n                }else{\r\n                    clearStatus(e);\r\n                }\r\n            }\r\n            div.classList.remove('dragging');\r\n        }\r\n\r\n        function clearStatus(e) {\r\n            var div = event.target || window.event.srcElement;\r\n            while (!div.classList.contains('draggable')) {\r\n                div = div.parentNode;\r\n            }\r\n            uid = div.querySelector('.uid').textContent;\r\n            if (div.classList.contains('dragged-left-perm') || div.classList.contains('dragged-right-perm')) {\r\n                result = _get_api(\"/livemgn/clear/\" + uid);\r\n                if (result == \"ok\") {\r\n                    removeClassname(uid, 'dragged-left-perm')\r\n                    removeClassname(uid, 'dragged-right-perm')\r\n                }\r\n            }\r\n        }\r\n\r\n        function addClassname(uid, classname) {\r\n            var divs = Array.from(document.querySelectorAll('div'));\r\n            var uidDivs = divs.filter(element => element.className.includes('uid') && element.innerText === uid);\r\n            uidDivs.forEach(function (div) {\r\n                div = div.parentNode;\r\n                div.classList.add(classname);\r\n            });\r\n        }\r\n\r\n        function removeClassname(uid, classname) {\r\n            var divs = Array.from(document.querySelectorAll('div'));\r\n            var uidDivs = divs.filter(element => element.className.includes('uid') && element.innerText === uid);\r\n            uidDivs.forEach(function (div) {\r\n                div = div.parentNode;\r\n                div.classList.remove(classname);\r\n            });\r\n        }\r\n\r\n        function EmptyMessage(index) {\r\n            // 创建新的 div 元素\r\n            var newDiv = document.createElement(\"div\");\r\n            newDiv.className = \"draggable\";\r\n            //newDiv.classList.add(\"hidden_field\");\r\n            newDiv.style.top = (index * 10.5 + 4) + \"vh\";\r\n\r\n            // 创建并添加 uid div\r\n            var uidDiv = document.createElement(\"div\");\r\n            uidDiv.className = \"uid hidden_field\";\r\n            uidDiv.textContent = \"0\";\r\n            newDiv.appendChild(uidDiv);\r\n\r\n            // 创建并添加 rawinfo div\r\n            var rawinfoDiv = document.createElement(\"div\");\r\n            rawinfoDiv.className = \"rawinfo hidden_field\";\r\n            rawinfoDiv.textContent = \"\";\r\n            newDiv.appendChild(rawinfoDiv);\r\n\r\n            // 创建并添加 img 元素\r\n            var newImg = document.createElement(\"img\");\r\n            newImg.className = \"avatar\";\r\n            newImg.src = \"\";\r\n            newImg.alt = \"Avatar\";\r\n            newDiv.appendChild(newImg);\r\n\r\n            // 创建并添加 info div\r\n            var infoDiv = document.createElement(\"div\");\r\n            infoDiv.className = \"info\";\r\n\r\n            // 创建并添加 nickname div\r\n            var nicknameDiv = document.createElement(\"div\");\r\n            nicknameDiv.className = \"nickname\";\r\n            nicknameDiv.textContent = \"\";\r\n            infoDiv.appendChild(nicknameDiv);\r\n\r\n            // 创建并添加 message div\r\n            var messageDiv = document.createElement(\"div\");\r\n            messageDiv.className = \"message\";\r\n            messageDiv.textContent = \"\";\r\n            infoDiv.appendChild(messageDiv);\r\n\r\n            // 将 info div 添加到 newDiv\r\n            newDiv.appendChild(infoDiv);\r\n\r\n            // 将新的 div 元素添加到 DOM 中\r\n            document.body.appendChild(newDiv);\r\n            newDiv.addEventListener('mousedown', startDrag);\r\n            newDiv.addEventListener('mousemove', drag);\r\n            newDiv.addEventListener('mouseup', endDrag);\r\n            newDiv.addEventListener('touchstart', startDrag);\r\n            newDiv.addEventListener('touchmove', drag);\r\n            newDiv.addEventListener('touchend', endDrag);\r\n            newDiv.addEventListener('onclick', clearStatus);\r\n            return newDiv;\r\n        }\r\n\r\n        function _get_api(url) {\r\n            var xhr = new XMLHttpRequest();\r\n            xhr.open('GET', url, false);\r\n            xhr.send(null);\r\n            return xhr.responseText;\r\n        }\r\n\r\n        for (i = 0; i < 100; i++) {\r\n            var item = EmptyMessage(i);\r\n            items.push(item);\r\n            if (item.offsetTop + item.offsetHeight * 2 >= window.innerHeight) {\r\n                break;\r\n            }\r\n        }\r\n    </script>\r\n\r\n    <script>\r\n        var socket;\r\n        function connect() {\r\n            socket = new WebSocket('<<PLACEHOLDER_WEBSOCKET_URL>>');\r\n\r\n            socket.onmessage = function (event) {\r\n                var message = JSON.parse(event.data);\r\n\r\n                switch (message.type) {\r\n                    case 'normal':\r\n                        PushMessage(message.name, message.content, message.avatar, message.uid, message.spclass);\r\n                        break;\r\n                    case 'pinned':\r\n                        PushMessage(message.name, message.content, message.avatar, message.uid, message.spclass);\r\n                        break;\r\n                }\r\n            };\r\n\r\n            socket.onclose = function (e) {\r\n                console.log('Socket is closed. Reconnect will be attempted in 1 second.', e.reason);\r\n                PushMessage(\"系统消息\", \"与OpenDanmaki后端失联，正在努力恢复...\", \"/images/sysavartar.png\", 0, \"\");\r\n                setTimeout(function () {\r\n                    connect();\r\n                }, 1000);\r\n            };\r\n\r\n            socket.onerror = function (err) {\r\n                console.log('Socket is closed. Reconnect will be attempted in 1 second.', e.reason);\r\n                PushMessage(\"系统消息\", \"与OpenDanmaki后端失联，正在努力恢复...\", \"/images/sysavartar.png\", 0, \"\");\r\n                setTimeout(function () {\r\n                    connect();\r\n                }, 1000);\r\n            };\r\n\r\n            socket.onopen = function (e) {\r\n                console.log('Socket connected');\r\n                PushMessage(\"系统消息\", \"已连接OpenDanmaki\", \"/images/sysavartar.png\", 0, \"\");\r\n            };\r\n        }\r\n\r\n        //如果可以，请求屏幕常亮\r\n        if ('wakeLock' in navigator) {\r\n            navigator.wakeLock.request('screen');\r\n        }\r\n        connect();\r\n    </script>\r\n</body>\r\n\r\n</html>";
    }
}