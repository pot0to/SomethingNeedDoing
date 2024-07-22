-- Mutamixer v1.2 Dawntrail
-- !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- Requires YesAlready & TextAdvance
-- These plugins are used to skip gossip, confirm boxes and the small upgrade cutscene
-- !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- Requires an even distribution of materia in the FIRST FIVE bag slots of the FIRST bag
-- If uneven put the smallest stack in inventory slot 5 (the slot on the right)
-- Change the "loops" variable on line 12 to match the stack size of the materia in slot 5
-- Once inventory is prepared walk up to Mutamix and start the script
-- !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

local loops = 1 -- how many hand ins you want to do

local done = 0 -- how many loops the script has done
yield("/target Mutamix Bubblypots")
yield("/interact")
repeat
    yield("/wait 0.1")
until IsAddonReady("SelectIconString")
yield("/callback SelectIconString true 0")
repeat
    yield("/wait 0.1")
until IsAddonReady("TradeMultiple")

while done < loops do
    -- 1
    yield("/callback TradeMultiple false 1 0 48 0")
    yield("/callback TradeMultiple false 4")
    repeat
        yield("/wait 0.1")
    until IsAddonReady("InputNumeric")
    yield("/callback InputNumeric true 1")
    repeat
        yield("/wait 0.1")
    until IsAddonVisible("InputNumeric") == false
    
    -- 2
    yield("/callback TradeMultiple false 1 0 48 1")
    yield("/callback TradeMultiple false 4")
    repeat
        yield("/wait 0.1")
    until IsAddonReady("InputNumeric")
    yield("/callback InputNumeric true 1")
    repeat
        yield("/wait 0.1")
    until IsAddonVisible("InputNumeric") == false
    
    -- 3
    yield("/callback TradeMultiple false 1 0 48 2")
    yield("/callback TradeMultiple false 4")
    repeat
        yield("/wait 0.1")
    until IsAddonReady("InputNumeric")
    yield("/callback InputNumeric true 1")
    repeat
        yield("/wait 0.1")
    until IsAddonVisible("InputNumeric") == false
    
    -- 4
    yield("/callback TradeMultiple false 1 0 48 3")
    yield("/callback TradeMultiple false 4")
    repeat
        yield("/wait 0.1")
    until IsAddonReady("InputNumeric")
    yield("/callback InputNumeric true 1")
    repeat
        yield("/wait 0.1")
    until IsAddonVisible("InputNumeric") == false
    
    -- 5
    yield("/callback TradeMultiple false 1 0 48 4")
    yield("/callback TradeMultiple false 4")
    -- if you already have 4 materia selected the numeric box won't show up
    yield("/wait 0.2")
    
    -- turnin
    yield("/callback TradeMultiple false 0")
    yield("/wait 0.1")
    
    repeat
        yield("/wait 0.1")
    until IsAddonReady("TradeMultiple")
    done = done + 1
    
end
yield("/echo <se.1> Hand-ins complete: "..done)
