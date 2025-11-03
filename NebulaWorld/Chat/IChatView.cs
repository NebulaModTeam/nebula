namespace NebulaWorld.Chat;

using System;
using NebulaModel.DataStructures.Chat;

/// <summary>
/// Defines the interface for a chat view implementation (e.g., TMPro, IMGUI).
/// The view is responsible for rendering messages and capturing user input.
/// </summary>
public interface IChatView
{
    /// <summary>
    /// Gets a value indicating whether the view is currently active and visible.
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// Adds a new message to the view for rendering.
    /// </summary>
    /// <param name="message">The raw message data to display.</param>
    void AddMessage(RawChatMessage message);

    /// <summary>
    /// Remove the message from the view.
    /// </summary>
    /// <param name="message">The raw message data to remove.</param>
    void RemoveMessage(RawChatMessage message);

    /// <summary>
    /// Clears messages from the view based on a specific predicate.
    /// </summary>
    /// <param name="predicate">The condition to select messages for removal.</param>
    void ClearMessages(Func<RawChatMessage, bool> predicate);

    /// <summary>
    /// Shows the view (e.g., opens the chat window).
    /// </summary>
    void Show();

    /// <summary>
    /// Hides the view (e.g., closes the chat window).
    /// </summary>
    void Hide();

    /// <summary>
    /// Toggles the view's visibility.
    /// </summary>
    /// <param name="forceClosed">If true, forces the view to close.</param>
    void Toggle(bool forceClosed = false);

    /// <summary>
    /// Inserts text into the chat input field at the current caret position.
    /// </summary>
    /// <param name="text">The text to insert.</param>
    void InsertText(string text);

    /// <summary>
    /// Checks if the mouse pointer is currently over the view.
    /// </summary>
    /// <returns>True if the pointer is in the view, false otherwise.</returns>
    bool IsPointerIn();

    /// <summary>
    /// Fired when the user submits a message from the input field (e.g., by pressing Enter).
    /// The string parameter is the raw text from the input field.
    /// </summary>
    event Action<string> OnMessageSubmitted;
}
