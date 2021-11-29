module PrismaticHelperSlightlyBetterCutsceneTrigger
using ..Ahorn, Maple

@mapdef Trigger "PrismaticHelper/SlightlyBetterCutsceneTrigger" SlightlyBetterCutsceneTrigger(x::Integer, y::Integer, 
   width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight, cutscene::String="", deathCount::Integer=-1, onlyOnce::Bool=false)

const placements = Ahorn.PlacementDict(
   "Slightly Beter Cutscene Trigger (Prismatic Helper)" => Ahorn.EntityPlacement(
      SlightlyBetterCutsceneTrigger,
      "rectangle",
   ),
)

end