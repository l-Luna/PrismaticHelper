module PrismaticHelperStylegroundsPanel

using ..Ahorn, Maple

@mapdef Entity "PrismaticHelper/StylegroundsPanel" StylegroundsPanel(
    x::Integer,
    y::Integer,
    width::Integer=Maple.defaultBlockWidth,
    height::Integer=Maple.defaultBlockWidth,
    foreground::Bool=false,
    scrollX::Number=1,
    scrollY::Number=1,
    opacity::Number=1,
    room::String="room",
    tint::String="#ffffff",
)

const placements = Ahorn.PlacementDict(
    "Stylegrounds Panel (Prismatic Helper)" => Ahorn.EntityPlacement(
        StylegroundsPanel, 
        "rectangle",
    ),
)

Ahorn.resizable(entity::StylegroundsPanel) = true, true

function Ahorn.selection(entity::StylegroundsPanel)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x, y, entity.width, entity.height)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::StylegroundsPanel, room::Maple.Room)
    x, y = entity.x, entity.y
    w, h = Int(entity.width), Int(entity.height)

    Ahorn.drawRectangle(ctx, 0, 0, w, h, (0.2, 0.6, 0.6, 0.6), (0.2, 0.6, 0.6, 0.4))
    Ahorn.drawCenteredText(ctx, "Stylegrounds Panel", 0, 0, w, h)
end

end
