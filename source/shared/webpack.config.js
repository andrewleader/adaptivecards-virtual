const path = require("path");

module.exports = (env, argv) => {
	const mode = argv.mode || 'development';
	const devMode = mode === "development";

	console.info('running webpack with mode:', mode);

	return {
		mode: mode,
		entry: {
			"shared": "./src/index.ts",
		},
		output: {
			path: path.resolve(__dirname, "dist"),
			filename: devMode ? "[name].js" : "[name].min.js",
			library: "Shared",
			libraryTarget: "var",
			globalObject: "this"
		},
		resolve: {
			extensions: [".ts", ".tsx", ".js"]
		},
		module: {
			rules: [
				{
					test: /\.ts$/,
					loader: "ts-loader",
					exclude: /(node_modules|__tests__)/
				}
			]
		}
	}
}