--script to kind of autofollow specific person in party when not in a duty by riding their vehicule
--meant to use when your ahh botting treasure maps or fates with alts, but playing main char manually :~D

--[[
*requirements:
croizats SND - disable SND targeting in config
simpletweaks with targeting fix enabled
vnavmesh
visland

*optional:
bring some gysahl greens
bring some food and configure it properly
discardhelper
lazyloot plugin (if your doing anything other than fates)
VBM/BMR (bmr has slash commands for following)
RSR (is RS still being updated?)

***Few annoying problems that still exist
*dont follow during combat unless non caster. will require bmr contemplation - seems bmr has contemplated it with distance command will consider adding new setting for this :~D
*how do we change instances #s maybe custom chat commands? lifestream /li # works. now to add nodetext scanning for group. also have to use target and lockon until lim fixes /li x without los
	this is insanely buggy and perhaps crashy.. nodetext scanning too fast will break things
*it still doesnt follow in some weird cases
*lazyloot is a toggle not on or off so you have to turn it on yourself
]]

--*****************************************************************
--************************* START INIZER **************************
--*****************************************************************
-- Purpose: to have default .ini values and version control on configs
-- Personal ini file
filename_prefix = "frenrider_" -- Script name
open_on_next_load = 0          -- Set this to 1 if you want the next time the script loads, to open the explorer folder with all of the .ini files

-- Function to open a folder in Explorer
function openFolderInExplorer(folderPath)
    if folderPath then
        folderPath = '"' .. folderPath .. '"'
        os.execute('explorer ' .. folderPath)
    else
        yield("/echo Error: Folder path not provided.")
    end
end

tempchar = GetCharacterName()
tempchar = tempchar:gsub("%s", "")  -- Remove all spaces
tempchar = tempchar:gsub("'", "")   -- Remove all apostrophes
local filename = os.getenv("appdata").."\\XIVLauncher\\pluginConfigs\\SomethingNeedDoing\\"..filename_prefix..tempchar..".ini"

if open_on_next_load == 1 then
    openFolderInExplorer(os.getenv("appdata").."\\XIVLauncher\\pluginConfigs\\SomethingNeedDoing\\")
end

function serialize(value)
    if type(value) == "boolean" then
        return tostring(value)
    elseif type(value) == "number" then
        return tostring(value)
    else -- Default to string
        return '"' .. tostring(value) .. '"'
    end
end

function deserialize(value)
    if value == "true" then
        return true
    elseif value == "false" then
        return false
    elseif tonumber(value) then
        return tonumber(value)
    else
        return value:gsub('^"(.*)"$', "%1")
    end
end

function read_ini_file()
    local variables = {}
    local file = io.open(filename, "r")
    if not file then
        return variables
    end

    for line in file:lines() do
        local name, val = line:match("([^=]+)=(.*)")
        if name and val then
            variables[name] = deserialize(val)
        end
    end
    file:close()
    return variables
end

function write_ini_file(variables)
    local file = io.open(filename, "w")
    if not file then
        yield("/echo Error: Unable to open file for writing: " .. filename)
        return
    end

    for name, value in pairs(variables) do
        file:write(name .. "=" .. serialize(value) .. "\n")
    end
    file:close()
end

function ini_check(varname, varvalue)
    local variables = read_ini_file()

    if variables["version"] and tonumber(variables["version"]) ~= vershun then
        yield("/echo Version mismatch. Recreating file.")
        variables = {version = vershun}
    end

    if variables[varname] == nil then
        variables[varname] = varvalue
        yield("/echo Initialized " .. varname .. " -> " .. tostring(varvalue))
    else
        yield("/echo Loaded " .. varname .. " -> " .. tostring(variables[varname]))
    end

    write_ini_file(variables)
    return variables[varname]
end

-- Ensure the version is always written to the file
-- VERSION VAR --
-- VERSION VAR --
-- VERSION VAR --
-- VERSION VAR --
vershun = 1                    -- Version number used to decide if you want to delete/remake the ini files on next load. useful if your changing party leaders for groups of chars or new version of script with fundamental changes
ini_check("version", vershun)
-- VERSION VAR --
-- VERSION VAR --
-- VERSION VAR --
-- VERSION VAR --

--*****************************************************************
--************************** END INIZER ***************************
--*****************************************************************

---------CONFIGURATION SECTION---------
fren = ini_check("fren", "Fren Name")  						--can be partial as long as its unique
fly_you_fools = ini_check("fly_you_fools", false)			--(fly and follow instead of mount and wait) usecase: you dont have multi seater of sufficient size, or you want to have multiple multiseaters with diff peopel riding diff ones.  sometimes frendalf doesnt want you to ride him and will ask you to ride yourself right up into outer space
fool_flier = ini_check("fool_flier", "Beast with 3 backs")	--if you have fly you fools as true, which beast shall you summon?
fulftype = ini_check("fulftype", "unchanged")				-- If you have lazyloot installed AND enabled (has to be done manually as it only has a toggle atm) can setup how loot is handled. Leave on "unchanged" if you don't want it to set your loot settings. Other settings include need, greed, pass
cling = ini_check("cling", 1) 								-- Distance to cling to fren when > bistance
force_gyasahl = ini_check("force_gyasahl", false) 	   		-- force gysahl green usage . maybe cause problems in towns with follow
clingtype = ini_check("clingtype", 0)						-- Clingtype, 0 = navmesh, 1 = visland, 2 = bmr follow leader, 3 = automaton autofollow, 4 = vanilla game follow
clingtypeduty = ini_check("clingtypeduty", 2)				-- do we need a diff clingtype in duties? use same numbering as above 
maxbistance = ini_check("maxbistance", 50) 					-- Max distance from fren that we will actually chase them, so that we dont get zone hopping situations ;p
limitpct = ini_check("limitpct", -1)						-- What percentage of life on target should we use LB at. It will automatically use LB3 if that's the cap or it will use LB2 if that's the cap, -1 disables it
rotationtype = ini_check("rotationtype", "Auto")			-- What RSR type shall we use?  Auto or Manual are common ones to pick. if you choose "none" it won't change existing setting.
bossmodAI = ini_check("bossmodAI", "on")					-- do we want bossmodAI to be "on" or "off"
feedme = ini_check("feedme", 4650)							-- eatfood, in this case itemID 4650 which is "Boiled Egg", use simpletweaks to show item IDs it won't try to eat if you have 0 of said food item
feedmeitem = ini_check("feedmeitem", "Boiled Egg")			-- eatfood, in this case the item name. for now this is how we'll do it. it isn't pretty but it will work.. for now..
--feedmeitem = ini_check("feedmeitem", "Baked Eggplant<hq>")-- eatfood, in this case the item name add a <hq> at the end if you want it to be hq. for now this is how we'll do it. it isn't pretty but it will work.. for now..
timefriction = ini_check("timefriction", 1)					-- how long to wait between "tics" of the main loop? 1 second default. smaller values will have potential crashy / fps impacts.
formation = ini_check("formation", false)					-- Follow in formation? If false, then it will "cling"
						--[[
						Like this -> . so that 1 is the main tank and the party will always kind of make this formation during combat
						8	1	5
						3		2
						7	4	6
						]]
--this next setting is a dud for now until i figure out how to do it
--seems like we will need to use puppetmaster.... ill carefully test this https://github.com/Aspher0/PuppetMaster_Fork
binstance = ini_check("binstance", "let us travel to instance")				--[[ group instance change prefix, it will take " x" where x is the instance number as an argument, so you can setup qolbar keys with lines like this presumable
after changing instances, followers will /cl their chat windows
exmample qolbar for telling group to go instance 2
/mount
/p let us travel to isntance 2
/li 2
]]
-- mker = "cross" -- In case you want the other shapes. Valid shapes are triangle square circle attack1-8 bind1-3 ignore1-2

-----------CONFIGURATION END-----------

----------------
--INIT SECTION--
----------------
yield("/echo Starting fren rider")
--yield("/target \""..fren.."\"")
yield("/wait 0.5")
--yield("/mk cross <t>")

yield("/vbmai "..bossmodAI)
yield("/bmrai "..bossmodAI)

if rotationtype ~= "none" then
	yield("/rotation "..rotationtype)
end

if fulftype ~= "unchanged" then
--turns out its just a toggle we can't turn it on or off purposefully
--	yield("/wait 0.5")
--	yield("/fulf on")
--	yield("/echo turning FULF ON!")
	yield("/echo Configuring FULF!")
	yield("/wait 1")
	yield("/fulf "..fulftype)
end
----------------
----INIT END----
----------------

--why is this so complicated? well because sometimes we get bad values and we need to sanitize that so snd does not STB (shit the bed)
function distance(x1, y1, z1, x2, y2, z2)
	if type(x1) ~= "number" then x1 = 0 end
	if type(y1) ~= "number" then y1 = 0 end
	if type(z1) ~= "number" then z1 = 0 end
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
    local dpsJobs = {
        [2]  = true, [4]  = true, [5]  = true, [7]  = true, [20] = true, [22] = true, [24] = true,
        [25] = true, [26] = true, [27] = true, [29] = true, [30] = true, [31] = true,
        [34] = true, [35] = true, [38] = true, [39] = true, [41] = true
    }
    local joeb = GetClassJobId()
    return dpsJobs[joeb] or false
end

function am_i_ranged()
	--*stub to be sorted out later to deal with known issue(s)
end

-- Function to calculate the offset based on follower index and leader's facing direction
function calculateOffset(followerIndex, leaderRotation)
    -- Calculate offsetX and offsetY based on follower index and leader's facing direction
    -- Example: Adjust offsetX and offsetY based on formation layout and leader's facing direction
    local offsetX, offsetY = 0, 0
    -- Adjust offsetX and offsetY based on formation layout and leader's facing direction
    if followerIndex == 1 then
        -- Example: Adjust offsetX and offsetY for follower 1
        offsetX, offsetY = -1 * cling * 2, cling * 2
    elseif followerIndex == 2 then
        -- Example: Adjust offsetX and offsetY for follower 2
        offsetX, offsetY = 0, cling * 2
    elseif followerIndex == 3 then
        -- Example: Adjust offsetX and offsetY for follower 3
        offsetX, offsetY = cling * 2, cling * 2
    elseif followerIndex == 4 then
        -- Example: Adjust offsetX and offsetY for follower 4
        offsetX, offsetY = -1 * cling * 2, 0
    -- Handle other follower indexes similarly
    end
    
    -- Rotate the offset based on the leader's facing direction
    local rotatedOffsetX = offsetX * math.cos(leaderRotation) - offsetY * math.sin(leaderRotation)
    local rotatedOffsetY = offsetX * math.sin(leaderRotation) + offsetY * math.cos(leaderRotation)
    
    return rotatedOffsetX, rotatedOffsetY
end

function moveToFormationPosition(followerIndex, leaderX, leaderY, leaderZ, leaderRotation)
    -- Calculate the formation position based on follower index and leader's facing direction
    local offsetX, offsetY = calculateOffset(followerIndex, leaderRotation)
    
    -- Move the follower to the formation position relative to the leader
    local targetX = leaderX + offsetX
    local targetY = leaderY + offsetY
    
    -- Example: Move follower to the calculated position
    PathfindAndMoveTo(targetX, targetY, leaderZ, false)
end

function clingmove(nemm)
	zclingtype = clingtype
	if GetCharacterCondition(34) == true then
		zclingtype = clingtypeduty --get diff clingtype in duties
	end
	--navmesh
	if zclingtype == 0 then
		PathfindAndMoveTo(GetObjectRawXPos(nemm),GetObjectRawYPos(nemm),GetObjectRawZPos(nemm), false)
	end
	--visland
	if zclingtype == 1 then
		yield("/visland moveto "..GetObjectRawXPos(nemm).." "..GetObjectRawYPos(nemm).." "..GetObjectRawZPos(nemm)) --* verify this is correct later when we can load dalamud
	end
	--not bmr
	if zclingtype > 2 or zclingtype < 2 then
			yield("/bmrai follow "..GetCharacterName())
	end
	--bmr
	if zclingtype == 2 then
		bistance = distance(GetPlayerRawXPos(), GetPlayerRawYPos(), GetPlayerRawZPos(), GetObjectRawXPos(nemm),GetObjectRawYPos(nemm),GetObjectRawZPos(nemm))
		if bistance < maxbistance then
			yield("/bmrai followtarget on") --* verify this is correct later when we can load dalamud
			yield("/bmrai follow "..nemm) 	  --* verify this is correct later when we can load dalamud
		end
		if bistance > maxbistance then --follow ourselves if fren too far away or it will do weird shit
			yield("/bmrai followtarget on") --* verify this is correct later when we can load dalamud
			yield("/bmrai follow "..GetCharacterName()) 	  --* verify this is correct later when we can load dalamud
			yield("/echo too far! stop following!")
		end
	end
	if zclingtype == 3 then
		yield("/autofollow "..nemm)
	end
	if zclingtype == 4 then
		--we only doing this silly method out of combat
		if GetCharacterCondition(26) == false then
			yield("/target "..nemm)
			yield("/follow")
		end
		--if we in combat and target is nemm we will clear it becuase that may bork autotarget from RSR
		if GetCharacterCondition(26) == true then
			if nemm == GetTargetName() then
				ClearTarget()
			end
		end
	end
end

weirdvar = 1
shartycardinality = 2 -- leader
partycardinality = 2 -- me
fartycardinality = 2 --leader ui cardinality
autotosscount = 0
we_are_in = GetZoneID()
we_were_in = GetZoneID()
for i=0,7 do
	if GetPartyMemberName(i) == fren then
		shartycardinality = i
	end
	if GetPartyMemberName(i) == GetCharacterName() then
		partycardinality = i
	end
end
partycardinality = partycardinality + 1
--turns out the above is worthless and not what i wanted for pillion. but we keep it anyways in case we need the data for something.

countfartula = 2
function counting_fartula()
countfartula = 2 --redeclare dont worry its fine.
	while countfartula < 9 do
		yield("/target <"..countfartula..">")
		yield("/wait 0.5")
		yield("/echo is it "..GetTargetName().."?")
		if GetTargetName() == fren then
			fartycardinality = countfartula
			countfartula = 9
			--yield("Aha... count fartula is -> "..fartycardinality)
		end
		countfartula = countfartula + 1
	end
end
counting_fartula() --we can call it before mounting because the order changes sometimes after a duty ends or after changing areas (AFTER a duty ends?) idk it was hard to recreate but this solves it.

--yield("Friend is party slot -> "..partycardinality.." but actually is ff14 slot -> "..fartycardinality)
yield("/echo Friend is party slot -> "..fartycardinality .. " Order of join -> "..partycardinality.." Fren Join order -> "..shartycardinality)
ClearTarget()

--bmr follow off. default state. slot1 is the runner of this script
--yield("/bmrai follow slot1")
yield("/bmrai follow slot1")
yield("/echo Beginning fren rider main loop")

while weirdvar == 1 do
	--catch if character is ready before doing anything
	if IsPlayerAvailable() then
		if type(GetCharacterCondition(34)) == "boolean" and type(GetCharacterCondition(26)) == "boolean" and type(GetCharacterCondition(4)) == "boolean" then
			bistance = distance(GetPlayerRawXPos(), GetPlayerRawYPos(), GetPlayerRawZPos(), GetObjectRawXPos(fren),GetObjectRawYPos(fren),GetObjectRawZPos(fren))
			if bistance > maxbistance then --follow ourselves if fren too far away or it will do weird shit
				clingmove(GetCharacterName())
			end

			--dismount regardless of in duty or not
			if IsPartyMemberMounted(shartycardinality) == false and fly_you_fools == true and GetCharacterCondition(4) == true then
				--continually try to dismount
				--bmr follow off.
				yield("/bmrai follow slot1")
				yield("/ac dismount")
				yield("/wait 0.5")
			end

			--Food check!
			statoos = GetStatusTimeRemaining(48)
			---yield("/echo "..statoos)
			if type(GetItemCount(feedme)) == "number" then
				if GetItemCount(feedme) > 0 and statoos < 300 then --refresh food if we are below 5 minutes left
					yield("/item "..feedmeitem)
					yield("/echo Attempting to eat "..feedmeitem)
				end
			end

			if GetCharacterCondition(34) == true then --in duty we might do some special things. mostly just follow the leader and let the ai do its thing.
				--bmr follow on
				--yield("/bmrai follow slot"..fartycardinality.."")
				--yield("/bmrai follow "..fren)
				--we will use clingmove not bmrai follow as it breaks pathing from that point onwards
				clingmove(fren)
			end
			if GetCharacterCondition(34) == false then  --not in duty  
				--SAFETY CHECKS DONE, can do whatever you want now with characterconditions etc			
				--movement with formation - initially we test while in any situation not just combat
				--check distance to fren, if its more than cling, then
	
				if formation == true and bistance < maxbistance then
					-- Inside combat and formation enabled
					local leaderX, leaderY, leaderZ = GetObjectRawXPos(fren), GetObjectRawYPos(fren), GetObjectRawZPos(fren)
					local leaderRotation = GetObjectRotation(fren)
					moveToFormationPosition(partycardinality, leaderX, leaderY, leaderZ, leaderRotation)
					yield("/wait 0.5")
				end
				--movement without formation
				if GetCharacterCondition(26) == true and formation == false then --in combat
					if formation == false then
						if bistance > cling and bistance < maxbistance then
						--yield("/target \""..fren.."\"")
							--PathfindAndMoveTo(GetObjectRawXPos(fren),GetObjectRawYPos(fren),GetObjectRawZPos(fren), false)
							clingmove(fren) --movement func
						end
						yield("/wait 0.5")
					end	
				end
				
				--we are limitbreaking all over ourselves
				if can_i_lb() == true and limitpct > -1 then
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
				if autotoss == true and autotosscount > 100 then
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
							
				--the code block that got this all started haha
				--follow and mount fren
				if GetCharacterCondition(26) == false then --not in combat
					if GetCharacterCondition(4) == true and fly_you_fools == true then
						--follow the fren
						if GetCharacterCondition(4) == true and bistance > cling and PathIsRunning() == false and PathfindInProgress() == false then
							--yield("/echo attempting to fly to fren")
							--bmr follow on. we comin
							--yield("/bmrai follow slot"..fartycardinality)
							--yield("/bmrai follow "..fren)
							clingmove(fren)

							yield("/target <"..fartycardinality..">")
							yield("/follow")
							yield("/wait 0.1") --we dont want to go tooo hard on this
							
							--i could't make the following method smooth please help :(
							--[[
							yield("/vnavmesh flyto "..GetObjectRawXPos(fren).." "..GetObjectRawYPos(fren).." "..GetObjectRawZPos(fren))
							looptillwedroop = 0
							while looptillwedroop == 0 do
								if PathIsRunning() == false and PathfindInProgress() == false then
									looptillwedroop = 1
									yield("/echo Debug Ok we reached path")
								end
								yield("/wait 0.1")
							end
							]]
						end
					end
					if GetCharacterCondition(4) == false and GetCharacterCondition(10) == false then --not mounted and not mounted2 (riding friend)
						--chocobo stuff. first check if we can fly. if not don't try to chocobo
						if HasFlightUnlocked() == true or force_gyasahl == true then
							--check if chocobro is up or (soon) not!
							if GetBuddyTimeRemaining() < 900 and GetItemCount(4868) > 0 then
								yield("/visland stop")
								yield("/vnavmesh stop")
								yield("/item Gysahl Greens")
								yield("/wait 3")
							end
						end
						--yield("/target <cross>")
						if formation == false then
							--check distance to fren, if its more than cling, then
							bistance = distance(GetPlayerRawXPos(), GetPlayerRawYPos(), GetPlayerRawZPos(), GetObjectRawXPos(fren),GetObjectRawYPos(fren),GetObjectRawZPos(fren))
							if bistance > cling and bistance < maxbistance then
							--yield("/target \""..fren.."\"")
								--PathfindAndMoveTo(GetObjectRawXPos(fren),GetObjectRawYPos(fren),GetObjectRawZPos(fren), false)
								clingmove(fren) --movement func
								--yield("/echo DEBUG line 467ish")
							end
							yield("/wait 0.5")
						end	
						--yield("/lockon on")
						--yield("/automove on")

						--[[yield("/ridepillion <"..mker.."> 1")
						yield("/ridepillion <"..mker.."> 2")
						yield("/ridepillion <"..mker.."> 3")]]
						--yield("/echo fly fools .."..tostring(fly_you_fools))
						if fly_you_fools == true then
							if GetCharacterCondition(4) == false and GetCharacterCondition(10) == false and IsPartyMemberMounted(shartycardinality) == true then
								--mountup your own mount
								yield("/mount \""..fool_flier.."\"")
								yield("/wait 5")
								--try to fly 
								yield("/gaction jump")
								yield("/lockon on")
							end
						end
						if IsPartyMemberMounted(shartycardinality) == true and fly_you_fools == false then
							--for i=1,7 do
								--yield("/ridepillion <"..partycardinality.."> "..i)
								counting_fartula()
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
	yield("/wait "..timefriction)
end