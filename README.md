# ERC20 Snapshot

An ERC20 scraper that captures all `Transfer` events to build a snap shot at any requested block.

# App Settings Setup

1. Update ChainInfo data to match local chain or remote chain

## API 

1. `~/snapShot` takes an optional `untilBlock` query param and downloads a zip of two files. `balances.csv` and `transfers.csv`. If not set, the latest synced block is used.


## Notes

1. If called in swagger, the file may not download. Call in browser directly.
2. The transfer events will be logged to the console. This can be replaced with social media or other actions.

# Version History

1. 2023-12-17: Initial release v1.0.0 

# Disclaimer

This implementation was made for educational / training purposes only.

# License

License is [MIT](https://en.wikipedia.org/wiki/MIT_License)

# MISC

Birbia is coming
