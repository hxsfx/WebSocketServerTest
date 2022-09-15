window.onload = function () {
    var TipElement = document.querySelector("#ChatContainer > div.tip");
    var MsgListElement = document.querySelector("#ChatContainer > div.msgList");
    var SendMsgContentElement = document.getElementById("SendMsgContent");
    var SendMsgButton = document.getElementById("SendMsgButton");

    window.wss = new WebSocket("ws://127.0.0.1:3300");//wss://xxx.hxsfx.com:xxx");
    //监听消息状态
    wss.onmessage = function (e) {
        var dataJson = JSON.parse(e.data);
        loadData(dataJson.nickName, dataJson.msg, dataJson.date, dataJson.time, true);
    }
    //监听链接状态
    wss.onopen = function () {
        if (TipElement.className.indexOf("conn") < 0) {
            TipElement.className = TipElement.className + " conn";
        }
        if (TipElement.className.indexOf("disConn") >= 0) {
            TipElement.className = TipElement.className.replace("disConn", "");
        }
    }

    //监听关闭状态
    wss.onclose = function () {
        if (TipElement.className.indexOf("conn") >= 0) {
            TipElement.className = TipElement.className.replace("conn", "");
        }
        if (TipElement.className.indexOf("disConn") < 0) {
            TipElement.className = TipElement.className + " disConn";
        }
    }

    //监控输入框回车键（直接发送输入内容）
    SendMsgContentElement.onkeydown = function () {
        if (event.keyCode == 13 && SendMsgContentElement.value.trim() != "") {
            if (SendMsgContentElement.value.trim() != "") {
                SendMsgButton.click();
                event.returnValue = false;
            } else {
                SendMsgContentElement.value = "";
            }

        }
    }
    //发送按钮点击事件
    SendMsgButton.onclick = function () {

        var msgDataJson = {
            msg: SendMsgContentElement.value,
        };
        SendMsgContentElement.value = "";
        var today = new Date();
        var date = today.getFullYear() + "年" + (today.getMonth() + 1) + "月" + today.getDate() + "日";
        var time = today.getHours() + ":" + today.getMinutes() + ":" + today.getSeconds();
        loadData("自己", msgDataJson.msg, date, time, false);

        let msgDataJsonStr = JSON.stringify(msgDataJson);
        wss.send(msgDataJsonStr);
    }

    //把数据加载到对话框中
    function loadData(nickName, msg, date, time, isOther) {
        let msgItemElement = document.createElement('div');
        if (isOther) {
            msgItemElement.className = "msgItem other";
        } else {
            msgItemElement.className = "msgItem self";
        }

        let chatHeadElement = document.createElement('div');
        chatHeadElement.className = "chatHead";
        chatHeadElement.innerHTML = "<svg viewBox=\"0 0 1024 1024\"><path d=\"M956.696128 512.75827c0 245.270123-199.054545 444.137403-444.615287 444.137403-245.538229 0-444.522166-198.868303-444.522166-444.137403 0-188.264804 117.181863-349.108073 282.675034-413.747255 50.002834-20.171412 104.631012-31.311123 161.858388-31.311123 57.297984 0 111.87909 11.128455 161.928996 31.311123C839.504032 163.650197 956.696128 324.494489 956.696128 512.75827L956.696128 512.75827M341.214289 419.091984c0 74.846662 38.349423 139.64855 94.097098 171.367973 23.119557 13.155624 49.151443 20.742417 76.769454 20.742417 26.64894 0 51.773154-7.096628 74.286913-19.355837 57.06467-31.113625 96.650247-96.707552 96.650247-172.742273 0-105.867166-76.664054-192.039781-170.936137-192.039781C417.867086 227.053226 341.214289 313.226864 341.214289 419.091984L341.214289 419.091984M513.886977 928.114163c129.883139 0 245.746984-59.732429 321.688583-153.211451-8.971325-73.739445-80.824817-136.51314-182.517917-167.825286-38.407752 34.55091-87.478354 55.340399-140.989081 55.340399-54.698786 0-104.770182-21.907962-143.55144-57.96211-98.921987 28.234041-171.379229 85.823668-188.368158 154.831344C255.507278 861.657588 376.965537 928.114163 513.886977 928.114163L513.886977 928.114163M513.886977 928.114163 513.886977 928.114163z\"></path></svg>";

        let msgMainElement = document.createElement('div');
        msgMainElement.className = "msgMain";
        let nickNameElement = document.createElement('div');
        nickNameElement.className = "nickName";
        nickNameElement.innerText = nickName;
        let msgElement = document.createElement('div');
        msgElement.className = "msg";
        msgElement.innerText = msg;
        let timeElement = document.createElement('div');
        timeElement.className = "time";
        let time_date_Element = document.createElement('span');
        time_date_Element.innerText = date;
        let time_time_Element = document.createElement('span');
        time_time_Element.innerText = time;
        timeElement.append(time_date_Element);
        timeElement.append(time_time_Element);
        msgMainElement.append(nickNameElement);
        msgMainElement.append(msgElement);
        msgMainElement.append(timeElement);


        msgItemElement.append(chatHeadElement);
        msgItemElement.append(msgMainElement);
        MsgListElement.append(msgItemElement);

        MsgListElement.scrollTop = MsgListElement.scrollHeight - MsgListElement.clientHeight;
    }
} 