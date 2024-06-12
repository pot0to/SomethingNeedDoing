-- Mutamixer v1.0
-- Requires YesAlready & TextAdvance to skip extra boxes and cutscenes
-- Requires an even distribution of materia in the first 5 bag slots of your first bag

-- TODO:
-- configurable inventory locations
-- turn copypasted pcalls into a function

local loops = 1 -- how many hand ins you want to do
local done = 0 -- how many the script has done

yield("/target Mutamix Bubblypots")
yield("/interact")
repeat
    yield("/wait 0.1")
until IsAddonReady("SelectIconString")
yield("/pcall SelectIconString true 0")
repeat
    yield("/wait 0.1")
until IsAddonReady("TradeMultiple")

while done < loops do
    -- 1
    yield("/pcall TradeMultiple false 1 0 48 0")
    yield("/pcall TradeMultiple false 4")
    repeat
        yield("/wait 0.1")
    until IsAddonReady("InputNumeric")
    yield("/pcall InputNumeric true 1")
    yield("/wait 0.1")
    
    -- 2
    yield("/pcall TradeMultiple false 1 0 48 1")
    yield("/pcall TradeMultiple false 4")
    repeat
        yield("/wait 0.1")
    until IsAddonReady("InputNumeric")
    yield("/pcall InputNumeric true 1")
    yield("/wait 0.1")
    
    -- 3
    yield("/pcall TradeMultiple false 1 0 48 2")
    yield("/pcall TradeMultiple false 4")
    repeat
        yield("/wait 0.1")
    until IsAddonReady("InputNumeric")
    yield("/pcall InputNumeric true 1")
    yield("/wait 0.1")
    
    -- 4
    yield("/pcall TradeMultiple false 1 0 48 3")
    yield("/pcall TradeMultiple false 4")
    repeat
        yield("/wait 0.1")
    until IsAddonReady("InputNumeric")
    yield("/pcall InputNumeric true 1")
    yield("/wait 0.1")
    
    -- 5
    -- if you already have 4 inputted the numeric box wont show up
    yield("/pcall TradeMultiple false 1 0 48 4")
    yield("/pcall TradeMultiple false 4")
    yield("/wait 0.1")
    
    -- turnin
    yield("/pcall TradeMultiple false 0")
    yield("/wait 0.1")
    
    repeat
        yield("/wait 0.1")
    until IsAddonReady("TradeMultiple")
    done = done + 1
end
yield("/echo <se.1> Hand-ins complete: "..done)
