var assert = require("assert");
var Shared = require("../dist/shared.min");

var unprocessedChanges = [];

onChanges = (changes) => {
    unprocessedChanges.push(changes);
}

describe("Test dist shared renderer", function () {
    it("Test initializing", function () {
        for (var p in exports) {
            console.log(p);
        }
        var renderer = new Shared.SharedRenderer();
        renderer.initialize(JSON.stringify({
            "type": "AdaptiveCard",
            "body": [
                {
                    "type": "TextBlock",
                    "text": "Hello world"
                }
            ]
        }), "{}");
        processChanges(1);
    });
});

function processChanges(amountToProcess) {
    for (var i = 0; i < amountToProcess; i++) {
        if (unprocessedChanges.length > 0) {
            unprocessedChanges.splice(0, 1);
        } else {
            assert.fail("There should have been a change, but there wasn't.");
        }
    }
    assert.equal(unprocessedChanges.length, 0, "There shouldn't have been any more changes left.");
}