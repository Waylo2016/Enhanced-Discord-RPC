console.log("YouTube Presence content script loaded");
function readState() {
  const video = document.querySelector("video");
  if (!video) return null;
  if (location.pathname === "/") return { status: "idle" };

  return {
    title: document.querySelector("h1 yt-formatted-string")?.textContent?.trim(),
    channel: document.querySelector("#owner #channel-name a")?.textContent?.trim() || null,
    channelLink: document.querySelector("#owner a")?.href,
    channelIcon: document.querySelector("#owner a img")?.src,
    videoLink: location.href,
    paused: video.paused,
    position: video.currentTime,
    duration: video.duration
  };
}

setInterval(() => {
  let state = readState();
  console.log("Current state:", state);
  if (state) chrome.runtime.sendMessage(state);
}, 1000);
