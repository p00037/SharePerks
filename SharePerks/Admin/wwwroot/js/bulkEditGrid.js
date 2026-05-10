window.sharePerksBulkEdit = {
    handleKeyDown: function (event, rowIndex, columnIndex, rowCount, columnCount) {
        const key = event.key;
        if (key !== "ArrowLeft" && key !== "ArrowRight" && key !== "ArrowUp" && key !== "ArrowDown") {
            return;
        }

        let nextRow = rowIndex;
        let nextColumn = columnIndex;

        if (key === "ArrowLeft") {
            nextColumn -= 1;
        } else if (key === "ArrowRight") {
            nextColumn += 1;
        } else if (key === "ArrowUp") {
            nextRow -= 1;
        } else if (key === "ArrowDown") {
            nextRow += 1;
        }

        if (nextRow < 0 || nextRow >= rowCount || nextColumn < 0 || nextColumn >= columnCount) {
            return;
        }

        event.preventDefault();
        const target = document.getElementById(`reward-item-bulk-edit-${nextRow}-${nextColumn}`);
        if (target) {
            target.focus();
            target.select();
        }
    }
};
