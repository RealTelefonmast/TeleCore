The VerbWatcher is a global instance working per active game.
When a game is loaded, any newly initialized Verb is tracked and receives a VerbAttacher.

When game is unloaded (main menu is entered) the VerbWatcher must be reset.

The purpose is to attach new custom logic onto existing Verbs without creating new Verb types, especially interesting for existing verbs that cannot easily be patched, like Verb_LaunchProjectile.

We must identify relevant hooks to inject or process Verb actions.

#### Patch Locations

Verb.TryStartCastOn() :

Verb.TryCastShot() :



