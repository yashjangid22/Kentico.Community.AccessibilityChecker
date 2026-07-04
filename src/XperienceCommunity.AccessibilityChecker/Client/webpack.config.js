const path = require("path");
const getWebpackConfig = require("@kentico/xperience-webpack-config");

module.exports = (env, argv) => {
    const baseConfig = getWebpackConfig({
        orgName: "xperiencecommunity",
        projectName: "accessibility.checker",
        webpackConfigPath: path.resolve(__dirname),
    });

    baseConfig.entry = "./src/index.tsx";

    baseConfig.module = {
        rules: [
            {
                test: /\.tsx?$/,
                use: "ts-loader",
                exclude: /node_modules/,
            },
        ],
    };

    baseConfig.resolve = {
        extensions: [".tsx", ".ts", ".js"],
    };

    return baseConfig;
};