# Original-platform implementation design

FNA has no working Xbox Live GamerServices provider. This directory preserves
the proposed sample without claiming portable service coverage.

On a licensed original-XNA platform, the screen would contain a sign-in state,
the current profile and picture, and a scrolling friends list. A small request
state machine would move through Idle, Loading, Ready, Failed and Canceled.
GamerServicesComponent would pump the provider; sign-in/out events would bump
a generation number so a completion from an earlier session cannot update the
current screen. Profile/picture requests would run asynchronously. Provider
results would be consumed on the game's update thread. Sign-out would cancel
owned operations, clear private data and release the picture texture.

Required validation: real provider-backed sign-in, profile and picture success,
an empty and a populated friends collection, sign-out during a pending request,
offline/provider failures and disposal on screen changes. A mock can exercise
the state machine during development, but cannot satisfy these acceptance
criteria. No credentials, provider impersonation or service emulation belong
in this repository.
