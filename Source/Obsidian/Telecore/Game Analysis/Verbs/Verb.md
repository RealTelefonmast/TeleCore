---
usedby: "[[VerbTracker]]"
---


Defines behavior of an attack or "cast" on any other entity.

TryCastOn() - Begins the Verb by specifying a target and params.
-> If the caster is a Pawn and the verb has a warmup, we check LOS, and then set the Stance of the pawn to Stance_Warmup, this causes the pawn to "prepare" the attack, and once it finishes without interrupting, tells the Verb that the warmup is complete with a WarmupComplete callback from the stance.
Otherwise if the caster is something like a turret, there is no stance to set, thats why turrets handle warmup manually and the verb simply calls WarmupComplete on itself on TryCastOn().

WarmupComplete() - Sets the verb state to "VerbState.Bursting" and calls TryCastNextBurstShot() which is to fire a first shot right on begin, instead of waiting for the VerbTick.

VerbTick - Does nothing until local state is set to Bursting, at which point it enters the Bursting logic:
Before each burst it checks for whether to stop the verb and Reset(), else it continues to count down until next burst shot, during that it always calls BurstingTick() - Essentially a ticker during the entire burst.
For each burst countdown reached it calls TryCastNextBurstShot(), which in itself checks for verb availability with Available(), and then also checks whether it can TryCastShot() - if both pass, it does various effects like muzzleflash, sound, pawn notifications like verb usage, mindstate changes, fuel use (if not a pawn), burstShotLeft decrement, cooldown state, or whether to stop bursting once no more shots are left.

=> TryCastNextBurstShot | Available && TryCastShot as entry point for single shot event?
