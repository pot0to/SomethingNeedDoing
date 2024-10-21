--this should be in the (Automaton) enhance duty/start end as /pcraft run start_gooning, obviously make the script in SND for this to work.
--set this to run when entering a duty
yield("/wait 2")
--loop wait for is char ready 
while IsPlayerAvailable() == false do
	yield("/echo waiting on player")
	yield("/wait 1")
end
yield("/ad stop")

--optional part if your having issues with magitek skillis being spammed and interaction slow downs over time.
--the following collection should contain pandora AND autoduty
--this is not a safe solution and sometimes breaks the plugin when re enabling (REEEEEEEE enabling)
--[[
yield("/xldisableprofile ad_collection")
while HasPlugin("AutoDuty") or HasPlugin("PandorasBox")  do
	yield("/wait 1")
end
yield("/wait 1")
yield("/xlenableprofile ad_collection")
--]]
yield("/wait 2")
yield("/hold W <wait.2.0>")
yield("/release W")
--[[
--while not (HasPlugin("AutoDuty") and HasPlugin("Pandora")) do
while not HasPlugin("AutoDuty") do
	yield("/wait 1")
	yield("/echo waiting on collection to turn back on")
end
--]]
yield("/echo ad start")
yield("/ad start")
yield("/vbm ai cfg on")
yield("/bmrai on")
yield("/rotation auto")
yield("/echo let's start gooning!")