--script to kind of autofollow specific person in party when not in a duty by riding their vehicule
--meant to use when your ahh botting treasure maps :~D

--[[
*requirements:
croizats SND - disable SND targeting in config
simpletweaks with targeting fix enabled
vnavmesh
visland

*optional:
bring some gysahl greens
lazyloot plugin (if your doing anything other than fates)

***Few annoying problems that still exist
1. sometimes it will wander off after an area change. i think this is a trailing moveto from the previous area.  and only happens if the leader and the follower teleport at same / almost same time to diff places
solution here is to check for area changes and do a /visland stop  and a /vnavmesh stop  if we notice an area change
**solution implemented needs testing

2. RS attacks training dummies which is a never ending source of annoyance in housing wards
NoHostileNames.json is the solution. gonna make this and include it i guess
no. "NostraThomas" has promised to add a toggle to ignore dummies . so we'll just wait for that

3. Players stacking up on master looks really bad
maybe can add a spread coefficient and include it as part of the script startup. a nice way would be to sort the names alphabetically and assign clock spots based on that
like this -> . so that 1 is the main tank and the party will always kind of make this formation. but only during combat ;~D
8	1	5
3		2
7	4	6
this would be on/off sort of thing
i have some nutty code i wrote for this that even takes object rotation into account to determine where to stand for the formation.

4. why aren't we limit breaking.... well maybe we will now hehehehe
implemented. needs testing
]]

---------CONFIGURATION SECTION---------
fren = "Frend Name" 	--can be partial as long as its unique
autotoss = true			--every 100 ticks it will try to auto discard whatever is on auto discard list. useful in treasure maps
fulftype = "unchanged"	--if you have lazyloot installed can setup how loot is handled. leave on "unchanged" if you dont want it to set your fulf settings. other setings include need, greed, pass
cling = 0.5 			--distance to cling to fren
clingy = true			--are we clingy? if not then the fren will have to swoop by to pick them up. recommend ON unless your doing quests or something.
limitpct = 25			--what pct of life on target should we use lb at. it will automatically use lb3 if thats the cap or it will use lb2 if thats the cap
formation = true		--follow in formation in combat?
						--[[
						like this -> . so that 1 is the main tank and the party will always kind of make this formation during combat
						8	1	5
						3		2
						7	4	6
						]]
--mker = "cross" --in case you want the other shapes. valid shapes are triangle square circle attack1-8 bind1-3 ignore1-2
-----------CONFIGURATION END-----------

--init
yield("/echo Starting fren rider")
--yield("/target \""..fren.."\"")
yield("/wait 0.5")
--yield("/mk cross <t>")

if fulftype ~= "unchanged" then
	yield("/fulf on")
	yield("/fulf "..fulftype)
end

--why is this so complicated? well because sometimes we get bad values and we need to sanitize that so snd does not STB (shit the bed)
local function distance(x1, y1, z1, x2, y2, z2)
	if type(x1) ~= "number" then x1 = 0 end
	if type(y1) ~= "number" then y1 = 0 end
	if type(z1) ~= "number" then y1 = 0 end
	if type(x2) ~= "number" then x2 = 0 end
	if type(y2) ~= "number" then y2	= 0 end
	if type(z2) ~= "number" then z2 = 0 end
	zoobz = math.sqrt((x2 - x1)^2 + (y2 - y1)^2 + (z2 - z1)^2)
	if type(zoobz) ~= "number" then
		zoobz = 0
	end
    --return math.sqrt((x2 - x1)^2 + (y2 - y1)^2 + (z2 - z1)^2)
    return zoobz
end

function can_i_lb()
	dps = false
	joeb = GetClassJobId()
	if joeb == 2 then dps = true end
	if joeb == 4 then dps = true end
	if joeb == 5 then dps = true end
	if joeb == 7 then dps = true end
	if joeb == 20 then dps = true end
	if joeb == 22 then dps = true end
	if joeb == 24 then dps = true end
	if joeb == 25 then dps = true end
	if joeb == 26 then dps = true end
	if joeb == 27 then dps = true end
	if joeb == 29 then dps = true end
	if joeb == 30 then dps = true end
	if joeb == 31 then dps = true end
	if joeb == 34 then dps = true end
	if joeb == 35 then dps = true end
	if joeb == 38 then dps = true end
	if joeb == 39 then dps = true end
--	if joeb == 41 then dps = true end --painter
--	if joeb == 42 then dps = true end --voper
	return dps
end

weirdvar = 1
partycardinality = 2
autotosscount = 0
we_are_in = GetZoneID()
we_were_in = GetZoneID()
for i=0,7 do
	if GetPartyMemberName(i) == fren then
		partycardinality = i
	end
end
--turns out the above is worthless and not what i wanted for pillion. but we keep it anyways in case we need the data for something.
local fartycardinality = 2
local countfartula = 2
while countfartula < 9 do
	yield("/target <"..countfartula..">")
	yield("/wait 0.5")
	yield("/echo is it "..GetTargetName().."?")
	if GetTargetName() == fren then
		fartycardinality = countfartula
		countfartula = 9
	end
	countfartula = countfartula + 1
end

--yield("Friend is party slot -> "..partycardinality.." but actually is ff14 slot -> "..fartycardinality)
yield("Friend is party slot -> "..fartycardinality)
ClearTarget()


while weirdvar == 1 do
	--catch if character is ready before doing anything
	if IsPlayerAvailable() then
		if type(GetCharacterCondition(34)) == "boolean" and type(GetCharacterCondition(26)) == "boolean" and type(GetCharacterCondition(4)) == "boolean" then
			if GetCharacterCondition(34) == false then  --not in duty 
				--SAFETY CHECKS DONE, can do whatever you want now with characterconditions etc			
				
				--we are limitbreaking all over ourselves
				if can_i_lb == true then
					GetLimoot = 0 --init lb value. its 10k per 1 bar
					GetLimoot = GetLimitBreakCurrentValue()
					if type(GetLimoot) ~= "number" then  --error trap variable type because we dont like SND pausing
						GetLimoot = 0 --well its 0 if its 0
					end
					local_teext = "\"Limit Break\""
					--check the target life %
					if type(GetTargetHPP()) == "number" and GetTargetHPP() < limitpct then
						--seems like max lb is 1013040 when ultimate weapon buffs you to lb3 but you only have 30k on your bar O_o
						--anyways it will trigger if lb3 is ready or when lb2 is max and it hits lb2
						if (GetLimoot == (GetLimitBreakBarCount() * GetLimitBreakBarValue())) or GetLimoot > 29999 then
							yield("/rotation Cancel")		
							yield("/echo Attempting "..local_teext)
							yield("/ac "..local_teext)
						end
						if GetLimoot < GetLimitBreakBarCount() * GetLimitBreakBarValue() then
							yield("/rotation auto")		
						end
						--yield("/echo limitpct "..limitpct.." HPP"..GetTargetHPP().." HP"..GetTargetHP().." get limoot"..GetLimitBreakBarCount() * GetLimitBreakBarValue()) --debug line
					end
				end

				autotosscount = autotosscount  + 1
				if autotoss == true and autotossc > 100 then
				   yield("/discardall")
				   autotosscount = 0
				end
				--check if we changed areas and stop movement and clear target
				we_are_in = GetZoneID()
				if we_are_in ~= we_were_in then
					yield("/wait 0.5")
					yield("/visland stop")
					yield("/vnavmesh stop")
					yield("/wait 0.5")
					yield("/visland stop")
					yield("/vnavmesh stop")
					ClearTarget()
					we_were_in = we_are_in
				end
				if GetCharacterCondition(26) == true then --in combat
						if clingy then
							--check distance to fren, if its more than cling, then
							bistance = distance(GetPlayerRawXPos(), GetPlayerRawYPos(), GetPlayerRawZPos(), GetObjectRawXPos(fren),GetObjectRawYPos(fren),GetObjectRawZPos(fren))
							if bistance > cling and bistance < 20 then
							--yield("/target \""..fren.."\"")
								PathfindAndMoveTo(GetObjectRawXPos(fren),GetObjectRawYPos(fren),GetObjectRawZPos(fren), false)
							end
							yield("/wait 0.5")
						end	
				end
				if GetCharacterCondition(26) == false then --not in combat
					if GetCharacterCondition(4) == false and GetCharacterCondition(10) == false then --not mounted and notmounted2 (riding friend)
						--chocobo stuff. first check if we can fly. if not don't try to chocobo
						if HasFlightUnlocked() == true then
							--check if chocobro is up or (soon) not!
							if GetBuddyTimeRemaining() < 900 and GetItemCount(4868) > 0 then
								yield("/visland stop")
								yield("/vnavmesh stop")
								yield("/item Gysahl Greens")
								yield("/wait 2")
							end
						end
						--yield("/target <cross>")
						if clingy then
							--check distance to fren, if its more than cling, then
							bistance = distance(GetPlayerRawXPos(), GetPlayerRawYPos(), GetPlayerRawZPos(), GetObjectRawXPos(fren),GetObjectRawYPos(fren),GetObjectRawZPos(fren))
							if bistance > cling and bistance < 20 then
							--yield("/target \""..fren.."\"")
								PathfindAndMoveTo(GetObjectRawXPos(fren),GetObjectRawYPos(fren),GetObjectRawZPos(fren), false)
							end
							yield("/wait 0.5")
						end	
						--yield("/lockon on")
						--yield("/automove on")

						--[[yield("/ridepillion <"..mker.."> 1")
						yield("/ridepillion <"..mker.."> 2")
						yield("/ridepillion <"..mker.."> 3")]]
						if IsPartyMemberMounted(partycardinality) == true then
							--for i=1,7 do
								--yield("/ridepillion <"..partycardinality.."> "..i)
								yield("/ridepillion <"..fartycardinality.."> 2")
							--end
							yield("/echo Attempting to Mount Friend")
							yield("/wait 0.5")
						end
					end
				end
			end
		end
	end
	yield("/wait 1")
end