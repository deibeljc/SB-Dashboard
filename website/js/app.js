var socket = new WebSocket("ws://localhost:8081");

$(document).ready(function() {
    /** Socket things */
    socket.onmessage = function(msg){
        var msgJson = JSON.parse(msg.data);
        // This is really shitty... I will eventually clean this up lol
        
        // Update the whole board
        var imageUrl = 'http://wow.zamimg.com/images/hearthstone/';
        if (_.get(msgJson, 'Hero') && _.get(msgJson, 'Enemy')) {
            $('.hero img').prop('src', imageUrl + 'heroes/' + msgJson.HeroId + '.png');
            $('.hero h4').html(msgJson.Hero.Template["<Name>k__BackingField"].split(" ")[0]);
            $('.hero span').html(msgJson.Hero.CurrentHealth + '/' + msgJson.Hero.MaxHealth);
            $('.enemy img').prop('src', imageUrl + 'heroes/' + msgJson.EnemyId + '.png');
            $('.enemy h4').html(msgJson.Enemy.Template["<Name>k__BackingField"].split(" ")[0]);
            $('.enemy span').html(msgJson.Enemy.CurrentHealth + '/' + msgJson.Enemy.MaxHealth);
        }

        $('wins').html(msgJson.Wins);
        $('losses').html(msgJson.Wins);

        _.each(msgJson.Log, function(message) {
            // Some app things
            var out = $('.log-scroll')[0];
            var isScrolledToBottom = out.scrollHeight - out.clientHeight <= out.scrollTop + 1;
            // Add the new log message
            $('.log-scroll').append('<li class="list-group-item">'+ message + '</li>');
            if(isScrolledToBottom) {
              out.scrollTop = out.scrollHeight - out.clientHeight;
          }
        });
    }
});

function sendStart() {
    socket.send("start");
}

function sendStop() {
    socket.send("stop");
}
