using System.Diagnostics;
using System.Threading.Tasks;

namespace BspImport.Builder;

public partial class MapBuilder
{
	protected ImportContext Context { get; set; }

	public MapBuilder( ImportContext context )
	{
		Context = context;

		SetupEntityHandlers();
	}

	/// <summary>
	/// Builds the final decompiled context insto the active editor scene. This will spawn world geometry and map entities, including parsed static props and brush entities.
	/// </summary>
	public async void Build( GameObject? parent = null )
	{
		using var progress = Editor.Application.Editor.ProgressSection( true );
		var token = progress.GetCancel();

		// begin scene population
		using var sceneScope = SceneEditorSession.Scope();

		var name = Context.Name;
		var root = new GameObject( parent, true, name );

		// start timer
		var stopwatch = new Stopwatch();
		stopwatch.Start();

		// for skybox culling
		Context.SkyboxAreas = FindSkyboxAreas();

		// build map worldspawn geometry (model 0), including displacements
		if ( Context.Settings.ImportWorldGeometry )
		{
			progress.Title = "Building World Geometry";
			progress.Subtitle = " Test";
			progress.Icon = "map";

			await GameTask.Delay( 100 );
			await BuildWorldGeometry( root, progress, meshesPerFrame: 64, token );
		}

		stopwatch.Stop();
		Log.Info( $"Build World took: {stopwatch.Elapsed.Seconds}s {stopwatch.Elapsed.Milliseconds}ms" );
		stopwatch.Restart();

		progress.Title = "Building Entities";

		// builds entities, including prop static and brush entities
		if ( Context.Settings.ImportEntities )
		{
			var entities = new GameObject( root, true, "Entities" );

			// prepares bsp model meshes (brush entities)
			await GameTask.Delay( 100 );
			var sw = new Stopwatch();
			sw.Start();

			await BuildModelMeshes( progress, token );

			sw.Stop();
			Log.Info( $"Build model meshes took: {sw.Elapsed.Seconds}s {sw.Elapsed.Milliseconds}ms" );

			await GameTask.Delay( 100 );
			await BuildEntities( entities, progress, entitiesPerFrame: 64, token );
		}

		stopwatch.Stop();
		Log.Info( $"Build Entities took: {stopwatch.Elapsed.Seconds}s {stopwatch.Elapsed.Milliseconds}ms" );

		var repoURL = "https://github.com/DoctorGurke/sbox-bsp-import";

		Log.Info( $"Report bugs or contribute @{repoURL}" );
		Log.Info( $"Imported Source 1 BSP File using sbox-bsp-import by DoctorGurke" );
	}

	public static async Task<T?> RunWithTimeout<T>(
	Func<Task<T>> taskFactory,
	int timeoutMs )
	{
		var task = taskFactory();
		var delay = Task.Delay( timeoutMs );

		var completed = await Task.WhenAny( task, delay );

		if ( completed == delay )
		{
			Log.Warning( "Operation timed out" );
			return default;
		}

		return await task;
	}
}
