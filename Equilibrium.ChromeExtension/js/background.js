window.onload = function () {
	try {
		$.connection.hub.url = "http://armozgovoy-mob:666/signalr";

		// Declare a proxy to reference the hub.
		var statusHub = $.connection.statusHub;

		statusHub.client.setDisbalance = function (name, message) {
			resetIcon(message);
        };

		$.connection.hub.logging = true;
		// Start the connection.
        $.connection.hub.start({ transport: ['webSockets', 'serverSentEvents','longPolling'] });
	} catch (e) {
		chrome.browserAction.setBadgeText({ text: 'Ошибка' });
		chrome.browserAction.setTitle({
			title: 'При подключении к серверу возникла ошибка...'
		});
    }
}

chrome.browserAction.setBadgeBackgroundColor({color:[190, 190, 190, 230]});
chrome.browserAction.setIcon({ path: 'images/gray_48.png' });
chrome.browserAction.setBadgeText({ text: '...' });
chrome.browserAction.setTitle({
	title: 'Выполняется подключение к серверу...'
});

var icons = ['images/red_48.png', 'images/yellow_48.png', 'images/green_48.png'];
function resetIcon(disbalance) {

    if (disbalance < -50) {
	    chrome.browserAction.setIcon({ path: icons[0] });

        chrome.browserAction.setBadgeText({ text: (-disbalance) + "ч"});
			chrome.browserAction.setTitle({
                title:'Текущий дисбаланс составляет '+ (-disbalance) +'ч. Интернет отключен за неуплату.'
    });
    } else if (-50 < disbalance < -30) {
	    chrome.browserAction.setIcon({ path: icons[1] });

	    chrome.browserAction.setBadgeText({ text: (-disbalance) + "ч" });
            chrome.browserAction.setTitle({ title: 'Текущий дисбаланс составляет ' + (-disbalance) + 'ч. Интернет будет отключен за неуплату через ' + (5 - Math.floor((-disbalance)/10)) +' дня.'});

    } else {
	    chrome.browserAction.setBadgeText({ text: (-disbalance) + "ч" });
	    chrome.browserAction.setIcon({ path: icons[2] });
	    chrome.browserAction.setTitle({ title: 'Текущий дисбаланс составляет ' + (-disbalance) + 'ч. Интернет будет отключен за неуплату через ' + (5 - Math.floor((-disbalance) / 10)) + ' дней.' });
	}
};