﻿// Copyright 2017 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using UnityEngine;
using System.Collections;
using DaydreamElements.Common;

namespace Daydream.MediaAppTemplate {

  /// This ScrollEffect is used to prevent FileSelectorTile from requesting a thumbnail from disk
  /// while the PagedScrollRect that it is a part of is currently scrolling quickly.
  public class FileSelectorScrollEffect : BaseScrollEffect {
    [SerializeField]
    private float maxMoveDistance = 100.0f;

    public override void ApplyEffect(BaseScrollEffect.UpdateData updateData) {
      FileSelectorPage page = updateData.page.GetComponent<FileSelectorPage>();
      if (page != null) {
        page.IsInTransition = updateData.moveDistance < maxMoveDistance;
      }
    }
  }
}
