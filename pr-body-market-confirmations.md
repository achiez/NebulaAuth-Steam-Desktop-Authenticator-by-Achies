## Why
Market sell confirmations can be grouped into a single row that only shows an item count (e.g. Sell 4 pcs). When selling different items, users can’t see what exactly is being confirmed or at what price.

## What changed
- Make grouped market sell confirmations expandable (eye icon) to reveal the underlying items.
- Show each item’s icon, name and price in the expanded list.
- Add per-item confirm/cancel actions, plus confirm-all/cancel-all on the group header.
- When items are confirmed/canceled individually, the group updates and automatically collapses back to a single confirmation when only one item remains.

## Test plan
- Open confirmations list.
- Verify grouped market sells show the eye icon and expand/collapse.
- Confirm/cancel a single item inside a group; ensure it’s removed from the group and the UI updates correctly.
- Confirm/cancel all on the group header still works.

