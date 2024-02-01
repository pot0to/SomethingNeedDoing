 --[[
  Description: party farm trial
  Author: McVaxius
]]

--we are now going to configure everything in a ini file.
--this way we can just copy paste the scripts and not need to edit the script per char

-- Function to load variables from a file
function loadVariablesFromFile(filename)
    local file = io.open(filename, "r")

    if file then
        for line in file:lines() do
            -- Extract variable name and value
            local variable, value = line:match("(%S+)%s*=%s*(.+)")
            if variable and value then
                -- Convert the value to the appropriate type (number or string)
                value = tonumber(value) or value
                _G[variable] = value  -- Set the global variable with the extracted name and value
            end
        end
        io.close(file)
    else
        print("Error: Unable to open file " .. filename)
    end
end

-- Specify the path to your text file
-- forward slashes are actually backslashes.
--to use this find the Trial_Farmer_MxVaxius.ini file and rename it to Trial_Farmer_Yourcharfirstlast.ini   notice no spaces.
--so if your character is named Pomelo Pup'per then you would call the .ini file   Trial_Farmer_PomeloPupper.ini
--also be sure to update the folder name as per your preference
--just remember it will strip spaces and apostrophes
tempchar = GetCharacterName()
--tempchar = tempchar:match("%s*(.-)%s*") --remove spaces at start and end only
tempchar = tempchar:gsub("%s", "")  --remove all spaces
tempchar = tempchar:gsub("'", "")   --remove all apostrophes
local filename = os.getenv("appdata").."\\XIVLauncher\\pluginConfigs\\SomethingNeedDoing\\Trial_Farmer_"..tempchar..".ini"

-- Call the function to load variables from the file
loadVariablesFromFile(filename)

-- Now you can use the variables in your Lua script
yield("/cl")
yield("/echo Character:"..GetCharacterName())
yield("/echo Filename+path:"..filename)
yield("/echo char_snake:"..char_snake)
yield("/echo enemy_snake:"..enemy_snake)
yield("/echo repeat_trial:"..repeat_trial)
yield("/echo repeat_type:"..repeat_type)
yield("/echo partymemberENUM:"..partymemberENUM)

--see the .ini file for explanation on settings
--more comments at the end

--counter init
local repeated_trial = 0

--our current area
local we_are_in = 666
local we_were_in = 666

--distance stuff
local snake_deest = 8 -- distance max to the specific char so we can decide when to start moving
local enemy_deest = 1 -- distance max to the specific enemy to beeline to the enemy using navmesh. set this to a higher value than snake_deest if you want it to never follow the enemy.
local meh_deest = 40 -- distance max to char_snake where we stop trying to follow or do anything. maybe look for interaction points or exits?
local enemeh_deest = 8 -- distance max to battle target

--placeholder for target location
local currentLocX = 1
local currentLocY = 1
local currentLocZ = 1

--our current location
local mecurrentLocX = GetPlayerRawXPos(tostring(1))
local mecurrentLocY = GetPlayerRawYPos(tostring(1))
local mecurrentLocZ = GetPlayerRawZPos(tostring(1))

--our current area
local we_are_in = 666
local we_were_in = 666

--declaring distance var
local dist_between_points = 500

--how long to lock on for. we only do it once every lockon_wait seconds. this will interfere less with VBM dodging mechanics
local dont_lockon = 0
local lockon_wait = 5

local neverstop = true
local i = 0

local function distance(x1, y1, z1, x2, y2, z2)
    return math.sqrt((x2 - x1)^2 + (y2 - y1)^2 + (z2 - z1)^2)
end

local function setdeest()
	if currentLocX and currentLocY and currentLocZ and mecurrentLocX and mecurrentLocY and mecurrentLocZ then
		dist_between_points = distance(currentLocX, currentLocY, currentLocZ, mecurrentLocX, mecurrentLocY, mecurrentLocZ)
		-- dist_between_points will contain the distance between the two points
		--yield("/echo Distance between char_snake and point 1: " .. dist_between_points)
	else
		yield("/echo Failed to retrieve coordinates for one or both points.")
		dist_between_points = 500 -- default value haha
	end
end

yield("/echo starting.....")

while repeated_trial < repeat_trial do
	yield("/targetenemy") --this will trigger RS to do stuff.
	if enemy_snake ~= "follow only" then --check if we are forcing a target or not
		yield("/target "..enemy_snake) --this will trigger RS to do stuff.
		currentLocX = GetTargetRawXPos()
		currentLocY = GetTargetRawYPos()
		currentLocZ = GetTargetRawZPos()
	end
	if enemy_snake == "follow only" then --follow mode loc
		currentLocX = GetPlayerRawXPos(tostring(char_snake))
		currentLocY = GetPlayerRawYPos(tostring(char_snake))
		currentLocZ = GetPlayerRawZPos(tostring(char_snake))
	end
	--yield("Target x y z "..currentLocX.." "..currentLocY.." "..currentLocZ)
	mecurrentLocX = GetPlayerRawXPos(tostring(1))
	mecurrentLocY = GetPlayerRawYPos(tostring(1))
	mecurrentLocZ = GetPlayerRawZPos(tostring(1))
	
	if GetCharacterCondition(34)==false and char_snake == "party leader" then --if we are not in a duty --try to restart duty
		yield("/visland stop")
		yield("/navmesh stop")
		yield("/wait 2")
		yield("/echo We seem to be outside of the duty.. let us enter!")
		yield("/wait 15")	
		if repeat_type == 0 then --4 Real players (or scripts haha) using duty finder
			yield("/finder")
			yield("/echo attempting to trigger duty finder")
			yield("/pcall ContentsFinder true 12 0")
		end
		if repeat_type == 1 then --just you using duty support
			--("/pcall DawnStory true 20") open the window.. how?
			--we use simpletweaks
			yield("/maincommand Duty Support")
			yield("/wait 2")
			yield("/echo attempting to trigger duty support")
			yield("/pcall DawnStory true 11 0") --change tab to first tab
			yield("/pcall DawnStory true 12 35")--select port decumana
			yield("/wait 2")
			yield("/pcall DawnStory true 14") --START THE DUTY
		end
	
		--reset duty specific stuff
		--nothing yet

		yield("/echo Total Trials triggered for "..char_snake..": "..repeated_trial)
		yield("/wait 10")
	end

	if GetCharacterCondition(26)==false and GetCharacterCondition(34)==true then --if we are not in combat AND we are in a duty then we will look for an exit
		yield("/target exit")
		yield("/wait 0.1")
		if GetTargetName()=="Exit" then --get out ! assuming pandora setup for auto interaction
			yield("/visland stop")
			yield("/wait 0.1")
			yield("/navmesh stop")
			yield("/wait 0.1")
			yield("/lockon on")
			yield("/automove on")
			yield("/wait 10")
		end
	end

	--test dist to the intended party leader
	if GetCharacterCondition(34)==true then --if we are in a duty
		if char_snake ~= "no follow" and enemy_snake ~= "nothing" then --dont close gaps to target if we are on no follow mode
			setdeest()
			if dist_between_points > snake_deest and dist_between_points < meh_deest then
					--yield("/visland moveto "..currentLocX.." "..currentLocY.." "..currentLocZ)
					yield("/vnavmesh moveto "..currentLocX.." "..currentLocY.." "..currentLocZ)
					--yield("/echo vnavmesh moveto "..math.ceil(currentLocX).." "..math.ceil(currentLocY).." "..math.ceil(currentLocZ))
			end
		end
		--duty specific stuff
		--porta decumana ultima weapon orbs in phase 2 near start of phase
		--very hacky kludge until movement isn't slidy
		--nested ifs because we don't want to get locked into this
		phase2 = distance(-692.46704, -185.53157, 468.43414, mecurrentLocX, mecurrentLocY, mecurrentLocZ)
		if phase2 < 20 and GetDistanceToObject("The Ultimate Weapon") < 20 then
			if partymemberENUM == 1 then
				yield("/vnavmesh moveto -692.46704 -185.53157 468.43414")
			end
			if partymemberENUM == 2 then
				yield("/vnavmesh moveto -715.5604 -185.53159 468.4341")
			end
			if partymemberENUM == 3 then
				yield("/vnavmesh moveto -715.5605 -185.53157 491.5273")
			end
			if partymemberENUM == 4 then
				yield("/vnavmesh moveto -692.46704 -185.53159 491.52734")
			end
			yield("/wait 5") -- is this too long? we'll see!
		end
--[[
--on hold for now until movement isnt slide-time-4000
		if type(GetDistanceToObject("Aetheroplasm")) == "number" then
--			if GetObjectRawXPos("Aetheroplasm") > 0 then
			if GetDistanceToObject("Aetheroplasm") < 20 then
				--yield("/wait 1")
				yield("/echo Porta Decumana ball dodger distance to random ball: "..GetDistanceToObject("Aetheroplasm"))
				yield("/visland stop")
				yield("/navmesh stop")
				--yield("/vbm cfg AI Enabled false")
				while type(GetDistanceToObject("Aetheroplasm")) == "number" and GetDistanceToObject("Aetheroplasm") < 20 do
					if partymemberENUM == 1 then
						yield("/visland moveto -692.46704 -185.53157 468.43414")
					end
					if partymemberENUM == 2 then
						yield("/visland moveto -715.5604 -185.53159 468.4341")
					end
					if partymemberENUM == 3 then
						yield("/visland moveto -715.5605 -185.53157 491.5273")
					end
					if partymemberENUM == 4 then
						yield("/visland moveto -692.46704 -185.53159 491.52734")
					end
					yield("/wait 5")			
				end
				yield("/visland stop")
				yield("/navmesh stop")
				--yield("/vbm cfg AI Enabled true")
			end
		end	
	]]
	end
	
	--test dist to the intended party leader
	i = 0
	if GetCharacterCondition(28)==true then --if we are bound by qte
		if GetCharacterCondition(29)==true then --if we are bound by qte
			while i < 150 do
				i = i + 1
				yield("/send SPACE")
				yield("/send SPACE")
				yield("/wait 0.1")
				if GetCharacterCondition(28)==false then --if we are not bound by qte get out of the space bar spamming so we can resume following or whatever
					i = 150
				end
			end
		end
	end
	yield("/wait 1")

	--this part will be deprecated soon once there is some kind of autobuilding
	--check if we chagned areas and rebuild navmesh or just wait as normal
	we_are_in = GetZoneID() --where are we?
	if type(we_are_in) ~= "number" then
		we_are_in = 666 --its an invalid type so lets just default it and wait 10 seconds
		yield("/echo invalid type for area waiting 10 seconds")
		yield("/wait 10")
	end
	if we_are_in ~= we_were_in then
		yield("/vnavmesh stop")
		yield("/wait 1")
		--if GetCharacterCondition(34) == true and char_snake ~= "no follow" then --only trigger rebuild in a duty and when following a party leader
		if GetCharacterCondition(34) == true then --only trigger rebuild in a duty and when following a party leader
			yield("/vnavmesh rebuild")
		end
		yield("/echo we changed areas. rebuild the navmesh!")
		repeated_trial = repeated_trial + 1
		--yield("/wait 10")
	end
	we_were_in = we_are_in --record this as we are in this area now
end


--just kind of proof of concept farming script for easy duties that vbm AI mode can solve
--make sure you go into settings and disable the snd targeting
--you need following plugins
--visland, SND (croizat fork), pandora
--simpletweaks -> turn on maincommand
--turn on auto interact on pandora set distance to 5 dist 5 height
--turn on vbm. click ai mode -> click follow. then form the group.
--turn on rotation solver if you like, set your lazyloot to /fulf need/green/pass/off etc
--preselect port decumana in the duty finder menu.
--meant for premade party but could be used for duty support
--enjoy

--/xldata object table, vbm debug, automaton debug
--gotta fix this up so it actually works.. the orb mechanic in phase 2 kind of fails too often as chars are not positioned correctly with 4 real players.
--17BB974B6D0:40000B89[38] - BattleNpc - Aetheroplasm - X-692.46704 Y-185.53157 Z468.43414 D9 R-0.7854581 - Target: E0000000
--17BB974E650:40000B8C[40] - BattleNpc - Aetheroplasm - X-715.5604 Y-185.53159 Z468.4341 D19 R0.78536224 - Target: E0000000
--17BB97515D0:40000B8A[42] - BattleNpc - Aetheroplasm - X-715.5605 Y-185.53157 Z491.5273 D21 R2.3561823 - Target: E0000000
--17BB9754550:40000B8B[44] - BattleNpc - Aetheroplasm - X-692.46704 Y-185.53159 Z491.52734 D12 R-2.3562784 - Target: E0000000

--some lbs for later maybe
--[[
/ac Final Heaven
/ac Dragonsong Dive
/ac Chimatsuri
/ac Doom of the Living
/ac The End
/ac Meteor
/ac Teraflare
/ac Vermilion Scourge
/ac Sagittarius Arrow
/ac Satellite Beam
/ac Crimson Lotus

/ac Bladedance
/ac Starstorm
/ac Desperado

/ac Braver
/ac Skyshard
/ac Big Shot
]]