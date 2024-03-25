--[[
script to fire up a map and go to it

requires
Pandora -> Auto-Teleport to Map Coords
Pandora -> auto interact + auto chests
Simpletweaks -> Target/Targetting fix
Globetrotter -> automatically get flag for opened map
Something Neeed Doing -> Yes you need this
Rotation Solver -> :D
Boss Mod -> :D
Lazyloot -> optional for "/fulf pass" or whatever you want to do with it
YesAlready -> the first time you decipher a map just add it to your yesalready

pseudocode
-> start
pop map
are we in same area already? if not tp to flag
flyto <flag>
pop dig
path to chest and pop it

functions:
public unsafe float GetFlagXCoord() => AgentMap.Instance()->FlagMapMarker.XFloat;
public unsafe float GetFlagYCoord() => AgentMap.Instance()->FlagMapMarker.YFloat;
public unsafe float GetFlagZone() => AgentMap.Instance()->FlagMapMarker.TerritoryId;
]]

--[[
maptypes
Level 80 Ostensibly Special Timeworn Map		33328
Level 90 Ostensibly Special Timeworn Map		??
]]

maptype = 33328

function distance(x1, y1, x2, y2)
    local dx = x2 - x1
    local dy = y2 - y1
    return math.sqrt(dx*dx + dy*dy)
end

function WalkTo(x, y, z)
    PathfindAndMoveTo(x, y, z, false)
    while (PathIsRunning() or PathfindInProgress()) do
        yield("/wait 0.5")
    end
end

local function safewait()
    repeat 
        yield("/wait 0.5")
        yield("/echo Are we ready? (backup check)")
    until IsPlayerAvailable()
	yield("/wait 1")
end
function ZoneTransition()
	repeat 
		yield("/wait 0.5")
		yield("/echo Casting TP")
	until not IsPlayerAvailable()
	repeat 
		yield("/wait 0.5")
		yield("/echo Arriving from TP")
	until IsPlayerAvailable()
end

--begin!
--yield("/item "..maptype)
--yield("/item Ostensibly Special Timeworn Map") --this doesnt actually work for some reason. it doesnt give an error though
yield("/gaction decipher")
yield("/wait 1")
yield("/pcall SelectIconString true 0")
safewait()

yield("target zone -> "..GetFlagZone())

if GetZoneID() ~= GetFlagZone() then
	--we need to get there!
    yield("/p <flag>")
	ZoneTransition()
end

--either we TPed there or we were there already.
--now lets get our butts to the place.  no wait first lets see if we are close enough to the flag already to just waltz over

-- if we are far from the flag (>5 yalm) path to it
tempdist = distance(GetPlayerRawXPos(), GetPlayerRawZPos(), GetFlagXCoord(),GetFlagYCoord())
yield("tempdist -> "..tempdist)
if tempdist > 5 then
	PathfindAndMoveTo(GetFlagXCoord(), GetPlayerRawYPos(), GetFlagYCoord(), false)
end

--pop dig
while GetCharacterCondition(4) do
	yield("/ac dismount")
	yield("/wait 2")
end
yield("/wait 1")
yield("/ac dig")
yield("/wait 7")
	
--target chest
yield("/target chest")
yield("/wait 0.5")
PathfindAndMoveTo(GetObjectRawXPos(GetTargetName()),GetObjectRawYPos(GetTargetName()),GetObjectRawZPos(GetTargetName()), false)
--move to chest

--DONE!