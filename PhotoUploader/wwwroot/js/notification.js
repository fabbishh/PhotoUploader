function showNotification(type, message) {
    // Создание элемента уведомления
    var notification = document.createElement("div");
    notification.classList.add("notification");
    notification.classList.add(type);

    // Добавление текста сообщения
    var messageElement = document.createElement("span");
    messageElement.classList.add("message")
    messageElement.textContent = message;
    notification.appendChild(messageElement);

    // Добавление кнопки закрытия
    var closeButton = document.createElement("span");
    closeButton.classList.add("close-button");
    closeButton.innerHTML = "&times;";
    closeButton.addEventListener("click", function () {
        notification.remove();
    });
    notification.appendChild(closeButton);

    // Добавление уведомления в контейнер
    var notificationContainer = document.getElementById("notificationContainer");
    notificationContainer.appendChild(notification);
}
