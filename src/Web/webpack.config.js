const path = require("path");
const webpack = require('webpack');

module.exports = {
    entry: __dirname + '/index.js',
    module: {
        rules: [
            {
                test: /\.jsx$/,
                exclude: /node_modules/,
                loader: 'babel-loader'
            }
        ]
    },
    output: {
        filename: 'transformed.js',
        path: path.join(__dirname, '/public'),
        publicPath: "/public/"
    },
    watch: true,
    devtool: "#eval-source-map",
    plugins: [
        new webpack.LoaderOptionsPlugin({
            debug: true
          }),
          new webpack.DefinePlugin({
              'process.env': {
                  NODE_ENV: JSON.stringify('development')
              }
          })
      ]
};