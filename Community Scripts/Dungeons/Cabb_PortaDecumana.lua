-- Porta Decumana Farmer
-- Requires VBM AI and rotation enabled or similar
-- Requires YesAlready auto dungeon queue accept or similar
-- Suggested user settings: Legacy movement, auto face target
-- no targeting for now, relies on other party members to engage the boss

-- important: change the homeArea variable to the area ID you're going to idle in
-- or your character will do nothing. default (341) is The Goblet

--vars
local queued = false
local toggled = false
local loops = 999
local done = 0
local homeArea = 341
local id = 0

-- searches for the Exit portal to let us know if the dungeon is done
function checkForExit()
    local namelist = GetNearbyObjectNames(5000)
    for i = 0, namelist.Count - 1 do
        if namelist[i] == "Exit" then
            return true
        end
    end
    return false
end

-- determines if the boss has the Vortex Shield buff that makes it immune to damage
function hasShield()
    if TargetHasStatus(3012) then
        return true
    end
    return false
end

-- first run setup
yield("/vbmai on")
OpenRegularDuty(830) -- open duty finder to porta decumana
yield("/waitaddon ContentsFinder")
yield("/pcall ContentsFinder false 12 1") -- press the clear button
yield("/wait 1")
yield("/pcall ContentsFinder false 3 4") -- tick porta decumana's checkbox
yield("/wait 1")
yield("/dutyfinder")

while done < loops do
    if IsPlayerAvailable() then -- idk if getzoneid needs this or not
        id = GetZoneID()
    end
    -- if we're in our home zone and not queued up, enter queue
    if id == homeArea and queued == false and IsPlayerAvailable() then
        yield("/wait 1")
        repeat
            OpenRegularDuty(830)
            yield("/waitaddon ContentsFinder")
            yield("/wait 1")
        until IsAddonVisible("ContentsFinder") -- safety check before pcall
        yield("/pcall ContentsFinder true 12 0")
        queued = true
    end
    -- if we're in the dungeon
    if id == 1048 and IsPlayerAvailable() then
        -- if we're in combat see if the boss has the immunity shield and stop attacking
        if GetCharacterCondition(26) then
            if GetTargetName() == "The Ultima Weapon" then
                if hasShield() and toggled == false then
                    toggled = true
                    yield("/vbmai off")
                end
                if hasShield() == false and toggled == true then
                    toggled = false
                    yield("/vbmai on")
                end
            end
        -- else check for the exit
        else
            if checkForExit() == true then
                yield("/wait 5") -- if you want to try to farm comms increase this wait to hang around a bit after the boss is dead
                queued = false
                yield("/pdfleave")
                done = done + 1
                yield("/echo Run #"..done.." completed! <se.1>")
            end
        end
    end
    -- end of loop wait
    yield("/wait 1")
end
