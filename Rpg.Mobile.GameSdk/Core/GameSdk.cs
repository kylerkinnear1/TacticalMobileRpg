namespace Rpg.Mobile.GameSdk.Core;

// TODO: Move to readme + markdown
// FLUENT API SDK for 2D Gaming. No base classes.
// Extensible via extension methods.
// Eventually, consider making a base game SDK, with a 2d or 3d background layer.
// Built in, centralized state management.
// Advanced Ideas (high performance games, minimize garbage collection):
// Containers, which are entities that group entities. These can have container level state management
// - share state within a container for arbitrary lifetime groups. Like a DI scope
// - can use state pool, so when a container is destroyed, all state is returned to pool.
// - IPooledVar<T> : IDisposable
//      - Release back to pool on dispose
//      - Heap only (since can't figure out which stack to put this on)
// - IScopedVar<T> (no IDisposable since disposed by lifetime system)
//      - Lifetimes: Global, Scene, Entity, Layer, Container, Local, Loop
//      - Both heap and stack scoped available
// - var scopedVar = _var.WithName<T>("name").Value;
// - _var.WithType<T>();
// - _var.WithId<T>(long);
// try and use stack for game loop, so that the stack is cleared after each loop.
// HeapStatePool. Pre-allocate some number of different types of state objects, when the (container is group of entities)
// Is there a way to use Span<byte[]> and cast it? I don't know how the garbage collector works, but I
// would guess if I cast and I don't new it up, it will work? Stack alloc means no garbage collection, so big performance improvement.

// SDK, not engine. Nuget package + VS code or Visual Studio extension eventually maybe for map editors,
// but those can be separate package.

// So, abstraction idea:
// Active Scene
// - IEnumerable<Layer> ForegroundLayers
// - Layer Background (add state adds to background layer by default)
// - ITilesets

// Loaded Scenes
// - IEnumerable<Layer> ForegroundLayers
// - Layer Background (add state adds to background layer by default)
// Stored Scenes
// - Return IScene from Load function.
// 
// Layer
// - Entity
// - Calculators
// - Mutators
// - Colliders (type of emmitter)
// - Emitter
// - Subscriber
// - Renderer2d (ICanvas canvas, RectF bounds);
// - Renderer3d (common 3d abstraction... whatever that looks like)
// - Renderer (base class. Just has Render() and you do everything on your end)
// - IRenderer2d (just calls this. You figure out your rendering engine on your end)

// IGameSdk
// - Assets
// Loading management
// Load scheduler

// - Audio
// - PlaySoundEffect (ISoundEffect : IPlayingSound)
// - LoopSound (ISoundLoop : IPlayingSound)

// - Scenes

// - OverrideRender (skips all renders, completely replaces screen)

// - Pause (Changes to Pause Scene. Have default pause scene layer.)

// - Create Tileset

// Publish Event

public interface IGameBuilder
{
    ISceneBuilder Scenes { get; }
    IAssetBuilder Assets { get; }
    IEntityBuilder Entities { get; }
    IAddOnSdk AddOns { get; }

    // TODO: Schedule Loader? One nice thing about this is extension methods
    // allow adding new functions to game builder with additional packages.
    // make all of this extension methods with some very base level concepts

    // game.AddOns.AddRenderer2d(x => { configureCode })
    // game.Scenes
    //  .AddScene<MyScene>(s => s
    //      .AddEntity<Tree>(e => e
    //          .AddRenderer2d(r => r.StaticImageFromFile("/tree.png")
    //      .AddEntity<Hero>(e => e
    //          .AddCalculator<HeroStateCalculator>()
    //          .AddRenderer2d(r => 
    //          {
    //              var idleTileSet = _tileBuilder.Load<TileSet>("hero_idle_tiles.png");
    //              var movingTileSet = _tileBuilder.Load<TileSet>("hero_moving_tiles.png");
    //              var sprite = _spriteBuilder.CreateSprite(ss => ss
    //                  .UseAnimation(idleTileSet, hero => hero.IsIdle)
    //                  .UseAnimation(movingTileSet, hero => hero.IsMoving);
    //              r.UseSpriteRenderer(sprite);
    //          })
    //          .AddParticleRenderer<SparkleParticles>();
}

// Marker interface for extension methods for add ons.
public interface IAddOnSdk
{

}
