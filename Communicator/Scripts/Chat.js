/* global $ */

var conversationManager = function () {

    this.usersArray = []; 
    this.domObject = $('#conversationsTabs');
    this.userStatusEnum = {
        'ONLINE': 1,
        'OFFLINE': 0
    }
    this.messageType = {
        'SELF': 0,
        'OTHER': 1
    }
    this.activeUserId = null;
    this.myUserName = 'Me';
    this.clientId = null;
}

conversationManager.prototype.addUser = function (userId, userName) {

    var foundUser = this.searchUser(userId);
    if (!foundUser && this.usersArray.indexOf(userId) < 0) {
        var user = {
            'id': userId,
            'name': userName,
            'messages': [],
            'status': this.userStatusEnum.ONLINE,
            'unreadedMessages': 0
        }
        this.usersArray.push(user);
    }

}

conversationManager.prototype.removeUser = function (userId) {

    alert('unimplement');

}

conversationManager.prototype.searchUser = function (userId) {

    for (var i = 0; i < this.usersArray.length; i++) {
        if (this.usersArray[i].id == userId) {
            return this.usersArray[i];
        }
    }

}

conversationManager.prototype.addMessage = function (userIdTo,userIdFrom, text, sendedTime) {
    var clientUserId = $('#useridfrom').val();//temp
    var messageType = this.messageType.OTHER;
    if (userIdFrom == clientUserId) {
        messageType = this.messageType.SELF;
    }

    var message = { 'text': text, 'type': messageType, 'sendedTime': sendedTime, };

    var userIdToSearch = messageType == this.messageType.OTHER ? userIdFrom : userIdTo;
    var obj = this.searchUser(userIdToSearch);
    obj.messages.push(message);

    var name = null;
    if (messageType == this.messageType.OTHER) {
        name = obj.name;
    }
    else {
        name = this.myUserName;
    }
    console.log(this);

    if (this.activeUserId == userIdFrom || this.activeUserId == userIdTo) {

        this.displayMessage(name, text, messageType, sendedTime);
    }
}

conversationManager.prototype.setTab = function (userId) {

    var dom = $('#discussion');
    var obj = this.searchUser(userId);
    dom.html('');
    this.activeUserId = obj.id;
    for (var i = 0; i < obj.messages.length; i++) {
        var message = obj.messages[i];
        var name = message.type == this.messageType.OTHER ? obj.name : this.myUserName;
        this.displayMessage(name, message.text, message.type, message.sendedTime);
    }
}

conversationManager.prototype.displayMessage = function (name, text,type, time) {
    var dom = $('#discussion');
    var classAppend = 'message';
    if (type == this.messageType.SELF) {
        classAppend = 'my-message';
    }
    dom.append(
         '<dl class="' + classAppend + '">' +
         '<dt>' + name + '</dt>' +
         '<dd>' + text + '</dd></dl>'
         );
}



function AddMessage(messageClass, messageTitle, messageText) {
    /*$('#discussion').append(
        '<dl class="' + messageClass + '">' +
        '<dt>' + messageTitle + '</dt>' +
        '<dd>' + messageText + '</dd></dl>'
        );*/
    console.log('[' + messageTitle + '] ' + messageText);
}

function ChangeConncectionStatus(status) {
    var domElement = $('#connection-status');
    domElement.removeClass();
    switch (status) {
        case 'connected':
            domElement.addClass('connection-status-ok');
            break;
        case 'reconnected':
            domElement.addClass('connection-status-ok');
            break;
        case 'disconnected':
            domElement.addClass('connection-status-bad');
            break;
        case 'connecting':
            domElement.addClass('connection-status-doing');
            break;
        case 'reconnecting':
            domElement.addClass('connection-status-doing');
            break;
        default:
            break;
    }
    domElement.html('[' + status + ']');
}

$(document).ready(function () {

            
            var conversationTabsManagerObject = new conversationManager;
            var connected = false;
            var chat = $.connection.chatHub;
            var clientUserId = $('#useridfrom').val();
            ChangeConncectionStatus('connecting');


            $('.contact').click(function (event) {
                var userId = $(this).attr('userid');
                var userName = $(this).attr('userName');
                conversationTabsManagerObject.addUser(userId, userName);
                conversationTabsManagerObject.setTab(userId);
                $('#useridto').val(userId);
                $('#message').removeAttr("disabled");
                $('#sendmessage').removeAttr("disabled");
                event.preventDefault();
            });


            $('#message').focus();

            chat.client.message = function (userIdTo,userIdFrom, message, sendedTime) {
                var classAppend = 'message';
                if (userIdFrom == clientUserId) {
                    classAppend = 'my-message';
                }
                AddMessage(classAppend, userIdFrom, sendedTime, message);
                conversationTabsManagerObject.addMessage(userIdTo,userIdFrom, message, sendedTime);
            };
            chat.client.serverMessage = function (message, sendedTime) {
                AddMessage('bad-message', 'Server Message', message);
            };

            chat.client.getToken = function (token) {
                $('#token').val(token);
            }

            chat.client.setActiveUsers = function (userListJson) {
                var userList = $.parseJSON(userListJson);
                console.log(userList);

                for (var i = 0; i < userList.length; i++) {
                    console.log('a[userId = "' + userList[i] + '"]');
                    $('a[userId="' + userList[i] + '"]').addClass('active-user');
                }
            }

            chat.client.disableActiveUser = function (userId) {
                $('a[userId="' + userId + '"]').removeClass('active-user');
            }

            chat.client.enableActiveUser = function (userId) {
                $('a[userId="' + userId + '"]').addClass('active-user');
            }

            $.connection.hub.start().done(function () {
                connected = true;
                ChangeConncectionStatus('connected');

                AddMessage('good-message', '', 'Connected');

                chat.server.getToken();
                chat.server.getHistory();
                chat.server.getActiveUsers();

                $('#sendmessage').click(function (event) {
                    event.preventDefault();

                    var message = $('#message').val();
                    var userIdTo = $('#useridto').val();
                    var token = $('#token').val()

                    chat.server.send(message,
                                     userIdTo,
                                     token);
                    $('#message').val('').focus();
                });
            });


            var tryingToReconnect = false;

            $.connection.hub.reconnecting(function () {
                tryingToReconnect = true;
                ChangeConncectionStatus('reconnecting');
            });

            $.connection.hub.reconnected(function () {
                tryingToReconnect = false;
                ChangeConncectionStatus('reconnected');
            });

            $.connection.hub.disconnected(function () {
                if (tryingToReconnect) {
                    connected = false;
                    AddMessage('bad-message', '', 'Disconnected.');
                    ChangeConncectionStatus('disconnected');
                }
            });

        });