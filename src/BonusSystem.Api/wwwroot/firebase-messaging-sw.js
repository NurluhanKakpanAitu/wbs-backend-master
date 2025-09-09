// Firebase Messaging Service Worker
importScripts('https://www.gstatic.com/firebasejs/10.7.1/firebase-app-compat.js');
importScripts('https://www.gstatic.com/firebasejs/10.7.1/firebase-messaging-compat.js');

// Firebase configuration
const firebaseConfig = {
    apiKey: "AIzaSyBRAi0H5lKxqPYStWrQPCIovBHPNytD3n4",
    authDomain: "world-bonus-system-6d14b.firebaseapp.com",
    projectId: "world-bonus-system-6d14b",
    storageBucket: "world-bonus-system-6d14b.firebasestorage.app",
    messagingSenderId: "1018515867174",
    appId: "1:1018515867174:web:0f9a22eb59f232050508e7",
    measurementId: "G-BZ4NMPGK5Q"
};

// Initialize Firebase
firebase.initializeApp(firebaseConfig);
const messaging = firebase.messaging();

// Handle background messages
messaging.onBackgroundMessage((payload) => {
    console.log('Received background message:', payload);

    const notificationTitle = payload.notification.title;
    const notificationOptions = {
        body: payload.notification.body,
        icon: payload.notification.icon || '/favicon.ico',
        badge: '/favicon.ico',
        tag: 'firebase-messaging-payload'
    };

    self.registration.showNotification(notificationTitle, notificationOptions);
});
