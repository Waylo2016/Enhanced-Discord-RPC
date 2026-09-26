const port = chrome.runtime.connectNative("com.youpresence.waylo.tech");

port.onDisconnect.addListener(() => {
  console.error("native host disconnected:", chrome.runtime.lastError?.message);
});

chrome.runtime.onMessage.addListener((state) => {
  port.postMessage(state);
});
