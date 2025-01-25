﻿namespace ScottPlot;

public class Multiplot : IMultiplot
{
    /// <summary>
    /// Number of subplots in this multiplot
    /// </summary>
    public int Count => Subplots.Count;

    public MultiplotSharedAxisManager SharedAxes { get; } = new();

    /// <summary>
    /// This list contains plots, logic for positioning them, and records of where they were last rendered
    /// </summary>
    private readonly List<Plot> Subplots = [];

    /// <summary>
    /// This logic is used to create the initial layout for subplots in the multiplot
    /// </summary>
    public IMultiplotLayout Layout { get; set; } = new MultiplotLayouts.Rows();

    public MultiplotLayoutSnapshot LastRender { get; } = new();

    /// <summary>
    /// Create a multiplot with no initial subplots
    /// </summary>
    public Multiplot()
    {

    }

    /// <summary>
    /// Create a multiplot with a single subplot
    /// </summary>
    public Multiplot(Plot plot)
    {
        AddPlot(plot);
    }

    public void RemovePlot(Plot plot)
    {
        Subplots.Remove(plot);
        LastRender.Forget(plot);
    }

    /// <summary>
    /// Reset this multiplot so it only contains the given plot
    /// </summary>
    public void Reset(Plot plot)
    {
        Subplots.Clear();
        AddPlot(plot);
    }

    /// <summary>
    /// Create a new plot, add it as a subplot, and return it
    /// </summary>
    public Plot AddPlot()
    {
        Plot plot = new();
        AddPlot(plot);
        return plot;
    }

    /// <summary>
    /// Add the given plot as a subplot into this multiplot
    /// </summary>
    public void AddPlot(Plot plot)
    {
        if (Subplots.Count > 0)
        {
            plot.PlotControl = Subplots.First().PlotControl;
        }

        if (Subplots.Count > 0)
        {
            Plot lastPlot = Subplots.Last();
            plot.FigureBackground.Color = lastPlot.FigureBackground.Color;
            plot.DataBackground.Color = lastPlot.DataBackground.Color;
        }

        Subplots.Add(plot);
    }

    /// <summary>
    /// Add (or remove) plots until the given number of subplots is achieved
    /// </summary>
    public Plot[] AddPlots(int total)
    {
        while (Count > total)
        {
            Subplots.RemoveAt(Subplots.Count - 1);
        }

        while (Count < total)
        {
            AddPlot();
        }

        return GetPlots();
    }

    /// <summary>
    /// Return the plot at the given index
    /// </summary>
    public Plot GetPlot(int index)
    {
        return Subplots[index];
    }

    /// <summary>
    /// Return all plots in this multiplot
    /// </summary>
    public Plot[] GetPlots()
    {
        return Subplots.ToArray();
    }

    /// <summary>
    /// Render the multiplot on a canvas inside the given rectangle.
    /// </summary>
    public void Render(SKCanvas canvas, PixelRect figureRect)
    {
        SharedAxes.UpdateSharedPlotAxisLimits();

        canvas.Clear();

        PixelRect[] subplotRectangles = Layout.GetSubplotRectangles(this, figureRect);
        if (subplotRectangles.Length != Subplots.Count)
        {
            throw new InvalidOperationException($"Layout returned {subplotRectangles.Length} rectangles for {Subplots.Count} subplots");
        }

        for (int i = 0; i < Subplots.Count; i++)
        {
            Plot plot = Subplots[i];
            plot.RenderManager.ClearCanvasBeforeEachRender = false;
            plot.Render(canvas, subplotRectangles[i]);
            LastRender.Remember(plot, subplotRectangles[i]);
        }
    }
}
