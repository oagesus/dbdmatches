"use client";

import { Button } from "@/components/ui/button";
import { ChevronLeft, ChevronRight, ChevronsLeft, ChevronsRight } from "lucide-react";

interface PaginationControlsProps {
  currentPage: number;
  totalPages: number;
  onPageChange: (page: number) => void;
}

function getPageRange(currentPage: number, totalPages: number): number[] {
  const windowSize = Math.min(5, totalPages);
  let start = currentPage - Math.floor(windowSize / 2);
  start = Math.max(1, start);
  start = Math.min(start, totalPages - windowSize + 1);
  return Array.from({ length: windowSize }, (_, i) => start + i);
}

export function PaginationControls({ currentPage, totalPages, onPageChange }: PaginationControlsProps) {
  if (totalPages <= 1) return null;

  const validPage = Math.min(Math.max(1, currentPage), totalPages);

  return (
    <div className="flex flex-col gap-1 pt-6">
      <div className="flex items-center justify-between md:justify-center md:gap-1">
        <Button
          variant="ghost"
          size="icon"
          className="hidden md:inline-flex h-8 w-8 cursor-pointer"
          onClick={() => onPageChange(1)}
          disabled={validPage === 1}
        >
          <ChevronsLeft className="h-4 w-4" />
        </Button>
        <Button
          variant="ghost"
          size="icon"
          className="h-8 w-8 cursor-pointer"
          onClick={() => onPageChange(validPage - 1)}
          disabled={validPage === 1}
        >
          <ChevronLeft className="h-4 w-4" />
        </Button>

        {getPageRange(validPage, totalPages).map((pageNum) => (
          <Button
            key={pageNum}
            variant={pageNum === validPage ? "default" : "ghost"}
            size="icon"
            className="h-8 w-8 text-xs cursor-pointer"
            onClick={() => onPageChange(pageNum)}
          >
            {pageNum}
          </Button>
        ))}

        <Button
          variant="ghost"
          size="icon"
          className="h-8 w-8 cursor-pointer"
          onClick={() => onPageChange(validPage + 1)}
          disabled={validPage === totalPages}
        >
          <ChevronRight className="h-4 w-4" />
        </Button>
        <Button
          variant="ghost"
          size="icon"
          className="hidden md:inline-flex h-8 w-8 cursor-pointer"
          onClick={() => onPageChange(totalPages)}
          disabled={validPage === totalPages}
        >
          <ChevronsRight className="h-4 w-4" />
        </Button>
      </div>
      <div className="flex items-center justify-between md:hidden">
        <Button
          variant="ghost"
          size="sm"
          className="h-7 gap-1 pl-0 text-xs text-muted-foreground cursor-pointer"
          onClick={() => onPageChange(1)}
          disabled={validPage === 1}
        >
          <ChevronsLeft className="h-3.5 w-3.5" />
          First
        </Button>
        <Button
          variant="ghost"
          size="sm"
          className="h-7 gap-1 pr-0 text-xs text-muted-foreground cursor-pointer"
          onClick={() => onPageChange(totalPages)}
          disabled={validPage === totalPages}
        >
          Last
          <ChevronsRight className="h-3.5 w-3.5" />
        </Button>
      </div>
      <div className="flex justify-center pt-1">
        <span className="text-xs text-muted-foreground">
          Page {validPage} of {totalPages}
        </span>
      </div>
    </div>
  );
}
