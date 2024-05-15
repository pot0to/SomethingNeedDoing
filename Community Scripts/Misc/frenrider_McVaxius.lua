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
none atm
]]

--*****************************************************************
--************************* START INIZER **************************
--*****************************************************************
--Purpose: to have default .ini values and version control on configs
--personal ini file -> 
filename_prefix = "frenrider_" --scriptname
open_on_next_load = 0		   --set this to 1 if you want the next time the script loads, to open the explorer folder with all of the .ini files
vershun = 1					   --version number used to decide if you want to delete/remake the ini files on next load. useful if your changing party leaders for groups of chars or new version of script with fundamental changes

-- Function to open a folder in Explorer
function openFolderInExplorer(folderPath)
    -- Check if folderPath is provided
    if folderPath then
        -- Enclose folderPath in double quotes to handle spaces in path
        folderPath = '"' .. folderPath .. '"'
        -- Execute the command to open the folder in Explorer
        os.execute('explorer ' .. folderPath)
    else
        yield("/echo Error: Folder path not provided.")
    end
end

tempchar = GetCharacterName()
--tempchar = tempchar:match("%s*(.-)%s*") --remove spaces at start and end only
tempchar = tempchar:gsub("%s", "")  --remove all spaces
tempchar = tempchar:gsub("'", "")   --remove all apostrophes
local filename = os.getenv("appdata").."\\XIVLauncher\\pluginConfigs\\SomethingNeedDoing\\"..filename_prefix.."_"..tempchar..".ini"

if open_on_next_load == 1 then
	openFolderInExplorer(os.getenv("appdata").."\\XIVLauncher\\pluginConfigs\\SomethingNeedDoing\\")
end

function serialize(value)
    if type(value) == "boolean" then
        return tostring(value)
    elseif type(value) == "number" then
        return tostring(value)
    else -- default to string
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

function ini_check(varname, varvalue)
    local file = io.open(filename, "r")
    if not file then
        file = io.open(filename, "w")
        if file then
            file:write(varname .. "=" .. serialize(varvalue) .. "\n")
            file:close()
            return varvalue
        else
            yield("/echo Error: Unable to create or open file for writing: " .. filename)
            return nil
        end
    end
    file:close()

    file = io.open(filename, "r")
    local foundVar = false
    local value = nil

    for line in file:lines() do
        local name, val = line:match("([^=]+)=(.*)")
        if name == varname then
            value = deserialize(val)
            foundVar = true
            break
        elseif name == "version" and tonumber(val) ~= vershun then
            yield("/echo Version mismatch. Recreating file.")
            file:close()
            os.remove(filename)
            return ini_check(varname, varvalue)
        end
    end
    file:close()

    if not foundVar then
        file = io.open(filename, "a")
        if file then
            file:write(varname .. "=" .. serialize(varvalue) .. "\n")
            file:close()
            return varvalue
        else
            yield("/echo Error: Unable to open file for writing: " .. filename)
            return nil
        end
    end

    return value
end


version = ini_check("version",vershun) --version number used to decide if you want to delete/remake the ini files on next load. useful if your changing party leaders for groups of chars or new version of script with fundamental changes
--*****************************************************************
--************************** END INIZER ***************************
--*****************************************************************

---------CONFIGURATION SECTION---------
fren = ini_check("fren", "Fren Name")  	--can be partial as long as its unique
fulftype = ini_check("fulftype", "unchanged")	--if you have lazyloot installed can setup how loot is handled. leave on "unchanged" if you dont want it to set your fulf settings. other setings include need, greed, pass
cling = ini_check("cling", 1) 				--distance to cling to fren
limitpct = ini_check("limitpct", 25)			--what pct of life on target should we use lb at. it will automatically use lb3 if thats the cap or it will use lb2 if thats the cap
formation = ini_check("formation", true)		--follow in formation? if false, then it will "cling"
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
        [2] = true, [4] = true, [5] = true, [7] = true, [20] = true, [22] = true, [24] = true,
        [25] = true, [26] = true, [27] = true, [29] = true, [30] = true, [31] = true,
        [34] = true, [35] = true, [38] = true, [39] = true--,
        -- [41] = true, --painter
        -- [42] = true, --voper
    }
    local joeb = GetClassJobId()
    return dpsJobs[joeb] or false
end

-- Function to calculate the offset based on follower index and leader's facing direction
local function calculateOffset(followerIndex, leaderRotation)
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

local function moveToFormationPosition(followerIndex, leaderX, leaderY, leaderZ, leaderRotation)
    -- Calculate the formation position based on follower index and leader's facing direction
    local offsetX, offsetY = calculateOffset(followerIndex, leaderRotation)
    
    -- Move the follower to the formation position relative to the leader
    local targetX = leaderX + offsetX
    local targetY = leaderY + offsetY
    
    -- Example: Move follower to the calculated position
    PathfindAndMoveTo(targetX, targetY, leaderZ, false)
end

weirdvar = 1
shartycardinality = 2 -- leader
partycardinality = 2 -- me
local fartycardinality = 2 --leader ui cardinality
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
yield("Friend is party slot -> "..fartycardinality .. " Order of join -> "..partycardinality.." Fren Join order -> "..shartycardinality)
ClearTarget()

while weirdvar == 1 do
	--catch if character is ready before doing anything
	if IsPlayerAvailable() then
		if type(GetCharacterCondition(34)) == "boolean" and type(GetCharacterCondition(26)) == "boolean" and type(GetCharacterCondition(4)) == "boolean" then
			if GetCharacterCondition(34) == false then  --not in duty 
				--SAFETY CHECKS DONE, can do whatever you want now with characterconditions etc			
				--movement with formation - initially we test while in any situation not just combat
				if formation == true then
					-- Inside combat and formation enabled
					local leaderX, leaderY, leaderZ = GetObjectRawXPos(fren), GetObjectRawYPos(fren), GetObjectRawZPos(fren)
					local leaderRotation = GetObjectRotation(fren)
					moveToFormationPosition(partycardinality, leaderX, leaderY, leaderZ, leaderRotation)
					yield("/wait 0.5")
				end
				--movement without formation
				if GetCharacterCondition(26) == true and formation == false then --in combat
					if formation == false then
						--check distance to fren, if its more than cling, then
						bistance = distance(GetPlayerRawXPos(), GetPlayerRawYPos(), GetPlayerRawZPos(), GetObjectRawXPos(fren),GetObjectRawYPos(fren),GetObjectRawZPos(fren))
						if bistance > cling and bistance < 20 then
						--yield("/target \""..fren.."\"")
							PathfindAndMoveTo(GetObjectRawXPos(fren),GetObjectRawYPos(fren),GetObjectRawZPos(fren), false)
						end
						yield("/wait 0.5")
					end	
				end
				
				--we are limitbreaking all over ourselves
				if can_i_lb() == true then
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
						if formation == false then
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
						if IsPartyMemberMounted(shartycardinality) == true then
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