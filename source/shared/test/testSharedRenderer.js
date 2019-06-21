var assert = require("assert");
var shared = require("../lib/index");

var unprocessedChanges = [];

onChanges = (changes) => {
    unprocessedChanges.push(changes);
}

describe("Test shared renderer", function () {
    it("Test initializing", function () {
        var renderer = new shared.SharedRenderer();
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

    
    it("Test input change that doesn't affect", function () {
        var renderer = new shared.SharedRenderer();
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
        renderer.updateInputValue("myInput", "Andrew");
        processChanges(0);
    });

    
    it("Test input change that re-renders", function () {
        var renderer = new shared.SharedRenderer();
        renderer.initialize(JSON.stringify({
            "type": "AdaptiveCard",
            "body": [
                {
                    "type": "TextBlock",
                    "text": "Hello {inputs.myInput}"
                }
            ]
        }), "{}");
        processChanges(1);
        renderer.updateInputValue("myInput", "Andrew");
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