-- Some actual vars
force_fishing = 0 -- Set to 1 if you want the default indexed char to fish whenever possible
gc_cleaning_safetystock = 50 -- How many inventory units before we do a cleaning
folderPath = os.getenv("appdata").."\\XIVLauncher\\pluginConfigs\\SomethingNeedDoing\\"

loadfiyel = os.getenv("appdata").."\\XIVLauncher\\pluginConfigs\\SomethingNeedDoing\\_functions.lua"
functionsToLoad = loadfile(loadfiyel)
functionsToLoad()

FUTA_processors = {} -- Initialize variable

-- 3D Table   {}[i][j][k]
FUTA_defaults = {
    {
        {"Firstname Lastname@Server", 0}, -- {}[i][1..2] -- Name@server and return type
        {"FISH", 0},                      -- {}[i][2][1..2] -- Level
        {"CLEAN", 100, 0, 0, 0},          -- {}[i][3][1..5] -- Chance to do random cleaning
        {"FUEL", 0, 0},                   -- {}[i][4][1..3] -- Fuel safety stock trigger
        {"TT", 0, 0},                     -- {}[i][5][1..3] -- Minutes of TT, NPC to play
        {"CUFF", 0},                      -- {}[i][6][1..2] -- Minutes of Cuff a Cur
        {"MRK", 0},                       -- {}[i][7][1..2] -- Number of Magitek Repair Kits
        {"FCB", "nothing", "nothing"},    -- {}[i][8][1..3] -- Refresh FC buffs
        {"PHV", 0, 100},                  -- {}[i][9][1..3] -- Personal house visit
        {"DUTY", "Teaspoon Dropping Closet", -5, 0} -- {}[i][10][1..4] -- Duty details
    }
}

-- Define the file path where serialized data is stored
fullPath = os.getenv("appdata") .. "\\XIVLauncher\\pluginConfigs\\SomethingNeedDoing\\FUTAconfig_McVaxius.lua"

-- Read and deserialize the data
serializedData = readSerializedData(fullPath)
deserializedTable = {}
if serializedData then
    deserializedTable = deserializeTable(serializedData)
	    -- Assign the deserialized table to FUTA_processors
    FUTA_processors = deserializedTable

    -- Check the deserialized table
    --yield("/echo Deserialized table:")
    --printTable(FUTA_processors)
else
    yield("/echo Error: Serialized data is nil.")
end

--loadfiyel2 = os.getenv("appdata").."\\XIVLauncher\\pluginConfi----gs\\SomethingNeedDoing\\FUTAconfig_McVaxius.lua"
--functionsToLoad2 = loadfile(loadfiyel2)
--functionsToLoad2()

function getRandomNumber(min, max)
    return math.random(min, max)
end

ungabungabunga() -- Get out of anything safely.

hoo_arr_weeeeee = -1 -- Who are we? Default to -1 for figuring out if new char or not

for i = 1, #FUTA_processors do
    if GetCharacterName(true) == FUTA_processors[i][1][1] then
        hoo_arr_weeeeee = i
    end
end

if hoo_arr_weeeeee == -1 then
    -- We have a new char to add to the table!
    FUTA_processors[#FUTA_processors + 1] = {}
    -- Initialize all levels with defaults
    for j = 1, #FUTA_defaults[1] do
        FUTA_processors[#FUTA_processors][j] = {}

        for k = 1, #FUTA_defaults[1][j] do
            FUTA_processors[#FUTA_processors][j][k] = FUTA_defaults[1][j][k]
        end
    end

    -- Assign the character name
    FUTA_processors[#FUTA_processors][1][1] = GetCharacterName(true)
	hoo_arr_weeeeee = #FUTA_processors --we added it, lets cardinality it
end

-- Check if any table data is missing and put in a default value
for i = 1, #FUTA_processors do
    --yield("/echo Type of FUTA_processors["..i.."]:" .. type(FUTA_processors[i]))
    for j = 2, #FUTA_defaults[1] do
        --yield("/echo Type of FUTA_processors["..i.."]["..j.."]:" .. type(FUTA_processors[i][j]))
        for k = 1, #FUTA_defaults[1][j] do
            --yield("/echo Type of FUTA_processors["..i.."]["..j.."]["..k.."]:" .. type(FUTA_processors[i][j][k]))
            if FUTA_processors[i][j][k] == nil then
                FUTA_processors[i][j][k] = FUTA_defaults[1][j][k]
            end
        end
    end
end



--[[
-- Function to recursively print table contents
function printTable(t, indent)
    indent = indent or ""
    if type(t) ~= "table" then
        yield("/echo " .. indent .. tostring(t))
        return
    end
    
    for k, v in pairs(t) do
        local key = tostring(k)
        if type(v) == "table" then
            yield("/echo " .. indent .. key .. " =>")
            printTable(v, indent .. "  ") -- Recursive call with increased indent
        else
            yield("/echo " .. indent .. key .. " => " .. tostring(v))
        end
    end
end

-- Debug: Output the contents of FUTA_processors
if #FUTA_processors == 0 then
    yield("/echo FUTA_processors is empty!")
else
    yield("/echo FUTA_processors contents:")
    for i = 1, #FUTA_processors do
        yield("/echo Entry " .. i .. ":")
        printTable(FUTA_processors[i], "  ")
    end
end
--]]

-- After tablebunga() call
tablebunga("FUTAconfig_McVaxius.lua", "FUTA_processors", folderPath)
yield("/echo tablebunga() completed successfully")

-- Begin to do stuff
wheeequeheeheheheheheehhhee = 0 -- Secret variable
yield("/echo Debug: Beginning to do stuff")

if os.date("!*t").hour % 2 == 0 and os.date("!*t").min < 15 then
    if os.date("!*t").min >= 1 then
        wheeequeheeheheheheheehhhee = 1
    end
end
yield("/echo Debug: Time check completed")

-- Determine who is the lowest level fisher of them all.
lowestID = 1
for i = 1, #FUTA_processors do
    if FUTA_processors[i][2][2] > 0 and FUTA_processors[i][2][2] < FUTA_processors[lowestID][2][2] then
        lowestID = i
    end
end
yield("/echo Debug: Lowest ID determined as " .. lowestID)

-- If the lowest guy is max level, we aren't fishing
if FUTA_processors[lowestID][2][2] == 100 and force_fishing == 0 then
    wheeequeheeheheheheheehhhee = 0
    yield("/echo Lowest char is max level so we aren't fishing")
end

if FUTA_processors[lowestID][2][2] == 0 and force_fishing == 0 then
    wheeequeheeheheheheheehhhee = 0
    yield("/echo Lowest char has fishing disabled so we aren't fishing")
end

-- It's fishing time
if wheeequeheeheheheheheehhhee == 1 then
    if GetCharacterCondition(31) == false then
        if GetCharacterCondition(32) == false then
            ungabungabunga() -- We really try hard to be safe here
            yield("/echo Debug: Preparing for fishing")
            
            loadfiyel2 = os.getenv("appdata").."\\XIVLauncher\\pluginConfigs\\SomethingNeedDoing\\FUTA_fishing.lua"
            functionsToLoad = loadfile(loadfiyel2)
            
            yield("/waitaddon _ActionBar <maxwait.600><wait.2>")
            fishing()
            yield("/echo Debug: Fishing completed")

            -- Drop a log file entry on the charname + Level
            local file = io.open(folderPath .. "FeeshLevels.txt", "a")
            if file then
                currentTime = os.date("*t")
                formattedTime = string.format("%04d-%02d-%02d %02d:%02d:%02d", currentTime.year, currentTime.month, currentTime.day, currentTime.hour, currentTime.min, currentTime.sec)
                FUTA_processors[lowestID][3][2] = GetLevel()
                file:write(formattedTime.." - ".."["..lowestID.."] - "..FUTA_processors[lowestID][3][1].." - Fisher Lv - "..FUTA_processors[lowestID][3][2].."\n")
                file:close()
                yield("/echo Text has been written to '" .. folderPath .. "FeeshLevels.txt'")
            else
                yield("/echo Error: Unable to open file for writing")
            end
            tablebunga("FUTAconfig_McVaxius.lua", "FUTA_processors", folderPath)
            yield("/echo Debug: Log file entry completed")
        end
    end
else
    -- Start of processing things when there is no fishing
    if FUTA_processors[hoo_arr_weeeeee][3][2] > 0 then
        if getRandomNumber(0, 99) < FUTA_processors[hoo_arr_weeeeee][3][2] then
            yield("/echo Debug: Inventory cleaning adjustment started")
            clean_inventory()
            ungabungabunga()
            -- If [3] was 100, we set it back down to 10 because 100 means a one-time guaranteed cleaning
            if FUTA_processors[hoo_arr_weeeeee][3][2] == 100 then
                FUTA_processors[hoo_arr_weeeeee][3][2] = 10
                tablebunga("FUTAconfig_McVaxius.lua", "FUTA_processors", folderPath)
                yield("/echo Debug: Inventory cleaning adjustment completed")
            end
        end
		--*check inventory size and do gcturnin shit 
    end
end

-- Stop beginning to do stuff
ungabungabunga()
yield("/echo Debug: Finished all processing")