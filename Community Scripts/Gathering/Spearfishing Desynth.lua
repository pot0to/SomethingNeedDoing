--[[
  Description: Spearfishing Auto Desynth
  The script allows you to have it running a visland route (while spearfishing) and when you get to a certain inventory amount it will pause for you and proceed to desynth all your collectables.
  Version: 2
  Author: LegendofIceman
]]

--Route name below
routename = "Earthbreak Aethersand"

--How many slots do you want open before it starts to desynth?
--Default is 5
slots_remaining = 5

-- Starts the route/resumes it if you had it paused in visland
::Fishingstart::
  yield("/visland exec "..routename)
  yield("/visland resume")
  yield("/wait 1")

-- Checks to see how much inventory space you have
::StartCount::
i_count = tonumber(GetInventoryFreeSlotCount())

-- Does a constant check to see if the current inventory amount is less than the amount of slots you wanted open
::InventoryTest::
while not (i_count <= slots_remaining) do
  yield("/echo Slots Remaining: "..i_count)
  yield("/wait 1")
  i_count = tonumber(GetInventoryFreeSlotCount())
end
yield("/echo Inventory has reached "..i_count)
yield("/echo Time to Desynth")

-- test to see if you're still in the spearfishing menu
::GatherTest::
if GetCharacterCondition(6) then
  yield("/visland pause")
  yield("/wait 5")
  goto GatherTest
end

-- Once out of spearfishing menu, dismounts and starts the process for desynthing the fish
::DesynthStart::
if not GetCharacterCondition(6) then
  yield("/wait 1")
  yield("/visland pause")
  yield("/wait 1")
    if GetCharacterCondition(4) then
      yield("/mount \"Company Chocobo\"")
      end
  yield("/wait 3")
  yield('/ac "Aetherial Reduction"')
  yield("/waitaddon PurifyItemSelector")  -- Need to another check for the mount here, just on the off chance it still pops up
    if GetCharacterCondition(4) then
  yield("/mount \"Company Chocobo\"") 
  yield("/wait 0.2")
  yield("/pcall PurifyItemSelector True -2")
  yield("/wait 0.2")
  yield('/ac "Aetherial Reduction"')
      yield("/waitaddon PurifyItemSelector")
  
  end
  yield("/pcall PurifyItemSelector true 12 0")
  yield("/waitaddon PurifyResult")
  yield("/pcall PurifyResult True 0")
  yield("/wait 4")
end

-- checks to see if you're desynth'ing yet, then once it's done, resumes the route/process
::DesynthAll::
if GetCharacterCondition(39) then
  yield("/wait 3")
  goto DesynthAll
end 
if not GetCharacterCondition(39) then
  yield("wait 3")
  yield("/pcall PurifyAutoDialog True -2")
  yield("/pcall PurifyItemSelector True -1")
  yield("/visland resume")
end

goto StartCount