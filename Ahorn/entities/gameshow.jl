module PrismaticHelperGameshow
using ..Ahorn, Maple

@mapdef Entity "PrismaticHelper/Gameshow" Gameshow(x::Integer, y::Integer, 
   questions::String="")

const placements = Ahorn.PlacementDict(
   "Gameshow (Prismatic Helper)" => Ahorn.EntityPlacement(
      Gameshow,
      "point"
   )
)

Ahorn.nodeLimits(entity::Gameshow) = 0, -1

function Ahorn.selection(entity::Gameshow)
    x, y = Ahorn.position(entity)
    nodes = get(entity.data, "nodes", ())

    res = Ahorn.Rectangle[Ahorn.Rectangle(x - 16, y - 16, 32, 32)]
    
    for node in nodes
        nx, ny = Int.(node)
        push!(res, Ahorn.Rectangle(nx - 16, ny - 16, 32, 32))
    end
    
    return res
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::Gameshow, room::Maple.Room)
    Ahorn.drawSprite(ctx, "PrismaticHelper/gameshow/controller", 0, 0)
    px, py = Ahorn.position(entity)
    nodes = get(entity.data, "nodes", ())
    
    for node in nodes
        nx, ny = Int.(node)
        Ahorn.drawSprite(ctx, "PrismaticHelper/gameshow/controller", nx - px, ny - py)
    end
end

end