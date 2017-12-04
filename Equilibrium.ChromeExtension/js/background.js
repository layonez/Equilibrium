window.onload = function () {
	try {
        $.connection.hub.url = "http://equilibrium.croc.ru/signalr";

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
	//i.Зеленый – выводится подсказка  « <дисбаланс по правилам расчета для блокировок> - у вас все списано»
	//ii.      Желтый – выводится подсказка  «<дисбаланс по правилам расчета для блокировок>  - у вас будет заблокирован интернет через 1(-3) дня, спишите время»
	//iii.      Красный –  выводится подсказка  « <дисбаланс по правилам расчета для блокировок>  - у вас заблокирован интернет, спишите время»
    if (disbalance < -50) {
	    chrome.browserAction.setIcon({ path: icons[0] });

        chrome.browserAction.setBadgeText({ text: (-disbalance) + "ч"});
			chrome.browserAction.setTitle({
                title: 'Текущий дисбаланс составляет ' + (-disbalance) +'ч. У вас заблокирован интернет, спишите время.'
    });
    } else if (-50 <= disbalance <= -10) {
	    chrome.browserAction.setIcon({ path: icons[1] });

        chrome.browserAction.setBadgeText({ text: (-disbalance) + "ч" });
        const daysToBlockInternet = (5 - Math.floor((-disbalance) / 10));
        
        chrome.browserAction.setTitle({ title: 'Текущий дисбаланс составляет ' + (-disbalance) + 'ч. Интернет будет заблокирован ' + getDayWord(daysToBlockInternet)});

    } else {
	    chrome.browserAction.setBadgeText({ text: (-disbalance) + "ч" });
        chrome.browserAction.setIcon({ path: icons[2] });

        const daysToBlockInternet = (5 - Math.floor((-disbalance) / 10));
        chrome.browserAction.setTitle({ title: 'Текущий дисбаланс составляет ' + (-disbalance) + 'ч. Интернет будет заблокирован ' + getDayWord(daysToBlockInternet)});
	}
};

function getDayWord(daysToBlockInternet) {
	let dayWord;
    switch (daysToBlockInternet) {
    case 0:
	        dayWord = ' сегодня';
	    break;
	case 1:
            dayWord = `через ${daysToBlockInternet} день`;
		break;
	case 5:
            dayWord = `через ${daysToBlockInternet} дней`;
        break;
	default:
            dayWord = `через ${daysToBlockInternet} дня`;
		break;
    }
	return dayWord;
}